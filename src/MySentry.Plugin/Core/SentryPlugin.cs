using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Configuration;

namespace MySentry.Plugin.Core;

/// <summary>
/// Main implementation of the Sentry plugin.
/// Provides a unified interface for all Sentry operations.
/// </summary>
public sealed class SentryPlugin : ISentryPlugin, IUserFeedbackCapture, ICronMonitor
{
    private readonly IHub _hub;
    private readonly SentryPluginOptions _options;
    private readonly ILogger<SentryPlugin> _logger;
    private PluginSentryUser? _currentUser;

    /// <summary>
    /// Creates a new instance of the Sentry plugin.
    /// </summary>
    /// <param name="hub">The Sentry hub.</param>
    /// <param name="options">The plugin options.</param>
    /// <param name="logger">The logger.</param>
    public SentryPlugin(
        IHub hub,
        IOptions<SentryPluginOptions> options,
        ILogger<SentryPlugin> logger)
    {
        _hub = hub;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsEnabled => _hub.IsEnabled;

    /// <inheritdoc/>
    public PluginSentryEventId LastEventId => new(_hub.LastEventId);

    /// <inheritdoc/>
    public IErrorCapture Errors => this;

    /// <inheritdoc/>
    public IPerformanceMonitor Performance => this;

    /// <inheritdoc/>
    public IBreadcrumbTracker Breadcrumbs => this;

    /// <inheritdoc/>
    public IUserContextProvider UserContext => this;

    /// <inheritdoc/>
    public IScopeManager Scopes => this;

    /// <inheritdoc/>
    public async Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        await _hub.FlushAsync(timeout).ConfigureAwait(false);
    }

    #region IErrorCapture

    /// <inheritdoc/>
    public PluginSentryEventId CaptureException(Exception exception)
    {
        var eventId = _hub.CaptureException(exception);
        _logger.LogDebug("Captured exception with event ID {EventId}", eventId);
        return new PluginSentryEventId(eventId);
    }

    /// <inheritdoc/>
    public PluginSentryEventId CaptureException(Exception exception, Action<ISentryScope> configureScope)
    {
        var eventId = _hub.CaptureException(exception, scope =>
        {
            configureScope(new SentryScopeWrapper(scope));
        });
        _logger.LogDebug("Captured exception with event ID {EventId}", eventId);
        return new PluginSentryEventId(eventId);
    }

    /// <inheritdoc/>
    public PluginSentryEventId CaptureMessage(string message, PluginSeverityLevel level = PluginSeverityLevel.Info)
    {
        var sentryLevel = SentryScopeWrapper.MapSeverityLevel(level);
        var eventId = _hub.CaptureMessage(message, sentryLevel);
        _logger.LogDebug("Captured message with event ID {EventId}", eventId);
        return new PluginSentryEventId(eventId);
    }

    /// <inheritdoc/>
    public PluginSentryEventId CaptureMessage(string message, PluginSeverityLevel level, Action<ISentryScope> configureScope)
    {
        var sentryLevel = SentryScopeWrapper.MapSeverityLevel(level);
        var eventId = _hub.CaptureMessage(message, scope =>
        {
            scope.Level = sentryLevel;
            configureScope(new SentryScopeWrapper(scope));
        });
        _logger.LogDebug("Captured message with event ID {EventId}", eventId);
        return new PluginSentryEventId(eventId);
    }

    #endregion

    #region IPerformanceMonitor

    /// <inheritdoc/>
    public ITransactionTracker StartTransaction(string name, string operation)
    {
        var transaction = _hub.StartTransaction(name, operation);
        _hub.ConfigureScope(scope => scope.Transaction = transaction);
        _logger.LogDebug("Started transaction {Name} ({Operation})", name, operation);
        return new TransactionTrackerWrapper(transaction);
    }

    /// <inheritdoc/>
    public ITransactionTracker StartTransaction(string name, string operation, Action<TransactionOptions> configure)
    {
        var options = new TransactionOptions();
        configure(options);

        var context = new Sentry.TransactionContext(name, operation);
        var transaction = _hub.StartTransaction(context, new Dictionary<string, object?>());

        foreach (var tag in options.Tags)
        {
            transaction.SetTag(tag.Key, tag.Value);
        }

        foreach (var extra in options.ExtraData)
        {
            transaction.SetExtra(extra.Key, extra.Value);
        }

        if (options.BindToScope)
        {
            _hub.ConfigureScope(scope => scope.Transaction = transaction);
        }

        _logger.LogDebug("Started transaction {Name} ({Operation})", name, operation);
        return new TransactionTrackerWrapper(transaction);
    }

    /// <inheritdoc/>
    public ISpanTracker? GetCurrentSpan()
    {
        var span = _hub.GetSpan();
        return span is null ? null : new SpanTrackerWrapper(span);
    }

    /// <inheritdoc/>
    public ITransactionTracker? GetCurrentTransaction()
    {
        ITransactionTracer? transaction = null;
        _hub.ConfigureScope(scope => transaction = scope.Transaction);
        return transaction is null ? null : new TransactionTrackerWrapper(transaction);
    }

    /// <inheritdoc/>
    public ISpanTracker? StartSpan(string operation, string? description = null)
    {
        var parentSpan = _hub.GetSpan();
        if (parentSpan is null)
        {
            _logger.LogDebug("No parent span found, cannot create child span");
            return null;
        }

        var span = parentSpan.StartChild(operation, description);
        return new SpanTrackerWrapper(span);
    }

    #endregion

    #region IBreadcrumbTracker

    /// <inheritdoc/>
    public void AddBreadcrumb(
        string message,
        string? category = null,
        string? type = null,
        PluginBreadcrumbLevel level = PluginBreadcrumbLevel.Info)
    {
        _hub.AddBreadcrumb(
            message,
            category,
            type,
            level: SentryScopeWrapper.MapBreadcrumbLevel(level));
    }

    /// <inheritdoc/>
    public void AddBreadcrumb(
        string message,
        string? category,
        string? type,
        IReadOnlyDictionary<string, string>? data,
        PluginBreadcrumbLevel level = PluginBreadcrumbLevel.Info)
    {
        _hub.AddBreadcrumb(
            message,
            category,
            type,
            data?.ToDictionary(x => x.Key, x => x.Value),
            SentryScopeWrapper.MapBreadcrumbLevel(level));
    }

    /// <inheritdoc/>
    public void AddHttpBreadcrumb(string method, string url, int? statusCode = null)
    {
        var data = new Dictionary<string, string>
        {
            ["method"] = method,
            ["url"] = url
        };

        if (statusCode.HasValue)
        {
            data["status_code"] = statusCode.Value.ToString();
        }

        _hub.AddBreadcrumb(
            $"{method} {url}",
            "http",
            "http",
            data,
            Sentry.BreadcrumbLevel.Info);
    }

    /// <inheritdoc/>
    public void AddNavigationBreadcrumb(string from, string to)
    {
        var data = new Dictionary<string, string>
        {
            ["from"] = from,
            ["to"] = to
        };

        _hub.AddBreadcrumb(
            $"Navigation: {from} -> {to}",
            "navigation",
            "navigation",
            data,
            Sentry.BreadcrumbLevel.Info);
    }

    /// <inheritdoc/>
    public void AddQueryBreadcrumb(string query, string category = "query")
    {
        _hub.AddBreadcrumb(
            query,
            category,
            "query",
            level: Sentry.BreadcrumbLevel.Info);
    }

    #endregion

    #region IUserContextProvider

    /// <inheritdoc/>
    public void SetUser(PluginSentryUser user)
    {
        _currentUser = user;
        _hub.ConfigureScope(scope =>
        {
            scope.User = SentryScopeWrapper.MapUser(user);
        });
        _logger.LogDebug("Set user {UserId}", user.Id);
    }

    /// <inheritdoc/>
    public void SetUserId(string userId)
    {
        SetUser(new PluginSentryUser(userId));
    }

    /// <inheritdoc/>
    public void SetUser(string userId, string? email = null, string? username = null)
    {
        SetUser(new PluginSentryUser
        {
            Id = userId,
            Email = email,
            Username = username
        });
    }

    /// <inheritdoc/>
    public void ClearUser()
    {
        _currentUser = null;
        _hub.ConfigureScope(scope =>
        {
            scope.User = new Sentry.SentryUser();
        });
        _logger.LogDebug("Cleared user");
    }

    /// <inheritdoc/>
    public PluginSentryUser? GetCurrentUser() => _currentUser;

    #endregion

    #region IScopeManager

    /// <inheritdoc/>
    public void ConfigureScope(Action<ISentryScope> configureScope)
    {
        _hub.ConfigureScope(scope =>
        {
            configureScope(new SentryScopeWrapper(scope));
        });
    }

    /// <inheritdoc/>
    public async Task ConfigureScopeAsync(Func<ISentryScope, Task> configureScope)
    {
        await _hub.ConfigureScopeAsync(async scope =>
        {
            await configureScope(new SentryScopeWrapper(scope)).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IDisposable PushScope()
    {
        return _hub.PushScope();
    }

    /// <inheritdoc/>
    public IDisposable PushScope<TState>(TState state) where TState : notnull
    {
        return _hub.PushScope(state);
    }

    /// <inheritdoc/>
    public void WithScope(Action<ISentryScope> action)
    {
        using var _ = _hub.PushScope();
        _hub.ConfigureScope(scope =>
        {
            action(new SentryScopeWrapper(scope));
        });
    }

    /// <inheritdoc/>
    public async Task WithScopeAsync(Func<ISentryScope, Task> action)
    {
        using var _ = _hub.PushScope();
        await _hub.ConfigureScopeAsync(async scope =>
        {
            await action(new SentryScopeWrapper(scope)).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    #endregion

    #region IUserFeedbackCapture

    /// <inheritdoc/>
    public void CaptureFeedback(PluginSentryEventId eventId, string name, string email, string comments)
    {
        Sentry.SentrySdk.CaptureUserFeedback(
            new Sentry.SentryId(eventId.Value),
            email,
            comments,
            name);
        _logger.LogDebug("Captured user feedback for event {EventId}", eventId);
    }

    /// <inheritdoc/>
    public void CaptureFeedback(PluginUserFeedback feedback)
    {
        Sentry.SentrySdk.CaptureUserFeedback(
            new Sentry.SentryId(feedback.EventId.Value),
            feedback.Email ?? string.Empty,
            feedback.Comments,
            feedback.Name ?? string.Empty);
        _logger.LogDebug("Captured user feedback for event {EventId}", feedback.EventId);
    }

    /// <inheritdoc/>
    public void CaptureUserFeedback(PluginSentryEventId eventId, string? email, string comments, string? name = null)
    {
        CaptureFeedback(eventId, name ?? string.Empty, email ?? string.Empty, comments);
    }

    #endregion

    #region ICronMonitor

    /// <inheritdoc/>
    public string CheckInProgress(string monitorSlug)
    {
        var checkInId = Sentry.SentrySdk.CaptureCheckIn(monitorSlug, Sentry.CheckInStatus.InProgress);
        _logger.LogDebug("Check-in started for monitor {MonitorSlug} with ID {CheckInId}", monitorSlug, checkInId);
        return checkInId.ToString();
    }

    /// <inheritdoc/>
    public void CheckInOk(string monitorSlug, string? checkInId = null)
    {
        Sentry.SentryId? id = string.IsNullOrEmpty(checkInId) ? null : Sentry.SentryId.Parse(checkInId!);
        Sentry.SentrySdk.CaptureCheckIn(monitorSlug, Sentry.CheckInStatus.Ok, id);
        _logger.LogDebug("Check-in complete for monitor {MonitorSlug}", monitorSlug);
    }

    /// <inheritdoc/>
    public void CheckInError(string monitorSlug, string? checkInId = null)
    {
        Sentry.SentryId? id = string.IsNullOrEmpty(checkInId) ? null : Sentry.SentryId.Parse(checkInId!);
        Sentry.SentrySdk.CaptureCheckIn(monitorSlug, Sentry.CheckInStatus.Error, id);
        _logger.LogDebug("Check-in failed for monitor {MonitorSlug}", monitorSlug);
    }

    /// <inheritdoc/>
    public void ExecuteJob(string monitorSlug, Action job)
    {
        var checkInId = CheckInProgress(monitorSlug);
        try
        {
            job();
            CheckInOk(monitorSlug, checkInId);
        }
        catch
        {
            CheckInError(monitorSlug, checkInId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteJobAsync(string monitorSlug, Func<Task> job)
    {
        var checkInId = CheckInProgress(monitorSlug);
        try
        {
            await job().ConfigureAwait(false);
            CheckInOk(monitorSlug, checkInId);
        }
        catch
        {
            CheckInError(monitorSlug, checkInId);
            throw;
        }
    }

    /// <inheritdoc/>
    public T ExecuteJob<T>(string monitorSlug, Func<T> job)
    {
        var checkInId = CheckInProgress(monitorSlug);
        try
        {
            var result = job();
            CheckInOk(monitorSlug, checkInId);
            return result;
        }
        catch
        {
            CheckInError(monitorSlug, checkInId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteJobAsync<T>(string monitorSlug, Func<Task<T>> job)
    {
        var checkInId = CheckInProgress(monitorSlug);
        try
        {
            var result = await job().ConfigureAwait(false);
            CheckInOk(monitorSlug, checkInId);
            return result;
        }
        catch
        {
            CheckInError(monitorSlug, checkInId);
            throw;
        }
    }

    #endregion

    #region Helper properties for simplified access

    /// <inheritdoc/>
    public void SetTag(string key, string value)
    {
        _hub.ConfigureScope(scope => scope.SetTag(key, value));
    }

    /// <inheritdoc/>
    public void SetExtra(string key, object? value)
    {
        _hub.ConfigureScope(scope => scope.SetExtra(key, value));
    }

    /// <inheritdoc/>
    public void SetContext(string key, object value)
    {
        _hub.ConfigureScope(scope => scope.Contexts[key] = value);
    }

    #endregion
}
