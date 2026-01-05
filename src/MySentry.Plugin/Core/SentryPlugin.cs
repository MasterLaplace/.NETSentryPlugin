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
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hub);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(hub);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(options);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(logger);
#endif

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
        // Note: Sentry SDK 6.0 FlushAsync doesn't accept CancellationToken
        // We respect the CancellationToken by checking it before the async operation
        cancellationToken.ThrowIfCancellationRequested();
        await _hub.FlushAsync(timeout).ConfigureAwait(false);
    }

    #region IErrorCapture

    /// <inheritdoc/>
    public PluginSentryEventId CaptureException(Exception exception)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(exception);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(exception);
#endif

        var eventId = _hub.CaptureException(exception);
        _logger.LogDebug("Captured exception with event ID {EventId}", eventId);
        return new PluginSentryEventId(eventId);
    }

    /// <inheritdoc/>
    public PluginSentryEventId CaptureException(Exception exception, Action<ISentryScope> configureScope)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(configureScope);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(exception);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(configureScope);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(message);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(message);
#endif

        var sentryLevel = SentryScopeWrapper.MapSeverityLevel(level);
        var eventId = _hub.CaptureMessage(message, sentryLevel);
        _logger.LogDebug("Captured message with event ID {EventId}", eventId);
        return new PluginSentryEventId(eventId);
    }

    /// <inheritdoc/>
    public PluginSentryEventId CaptureMessage(string message, PluginSeverityLevel level, Action<ISentryScope> configureScope)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(configureScope);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(message);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(configureScope);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(operation);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(name);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(operation);
#endif

        var transaction = _hub.StartTransaction(name, operation);
        _hub.ConfigureScope(scope => scope.Transaction = transaction);
        _logger.LogDebug("Started transaction {Name} ({Operation})", name, operation);
        return new TransactionTrackerWrapper(transaction);
    }

    /// <inheritdoc/>
    public ITransactionTracker StartTransaction(string name, string operation, Action<TransactionOptions> configure)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(operation);
        ArgumentNullException.ThrowIfNull(configure);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(name);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(operation);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(configure);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(message);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(message);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(message);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(message);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(method);
        ArgumentException.ThrowIfNullOrEmpty(url);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(method);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(url);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(from);
        ArgumentException.ThrowIfNullOrEmpty(to);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(from);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(to);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(query);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(query);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(user);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(user);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(userId);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(userId);
#endif

        SetUser(new PluginSentryUser(userId));
    }

    /// <inheritdoc/>
    public void SetUser(string userId, string? email = null, string? username = null)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(userId);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(userId);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configureScope);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(configureScope);
#endif

        _hub.ConfigureScope(scope =>
        {
            configureScope(new SentryScopeWrapper(scope));
        });
    }

    /// <inheritdoc/>
    public async Task ConfigureScopeAsync(Func<ISentryScope, Task> configureScope)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configureScope);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(configureScope);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(action);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(action);
#endif

        using var _ = _hub.PushScope();
        _hub.ConfigureScope(scope =>
        {
            action(new SentryScopeWrapper(scope));
        });
    }

    /// <inheritdoc/>
    public async Task WithScopeAsync(Func<ISentryScope, Task> action)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(action);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(action);
#endif

        using var _ = _hub.PushScope();
        await _hub.ConfigureScopeAsync(async scope =>
        {
            await action(new SentryScopeWrapper(scope)).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    #endregion

    #region IUserFeedbackCapture

    /// <inheritdoc/>
    public PluginSentryEventId CaptureFeedback(PluginSentryEventId eventId, string name, string email, string comments)
    {
        // Sentry SDK 6.0.0: CaptureUserFeedback removed, use CaptureFeedback instead
        // New signature returns SentryId and has out parameter for result
        var feedbackId = Sentry.SentrySdk.CaptureFeedback(
            comments,
            email,
            name,
            associatedEventId: new Sentry.SentryId(eventId.Value));
        _logger.LogDebug("Captured user feedback with ID {FeedbackId} for event {EventId}", feedbackId, eventId);
        return new PluginSentryEventId(feedbackId);
    }

    /// <inheritdoc/>
    public PluginSentryEventId CaptureFeedback(PluginUserFeedback feedback)
    {
        var feedbackId = Sentry.SentrySdk.CaptureFeedback(
            feedback.Comments,
            feedback.Email,
            feedback.Name,
            associatedEventId: new Sentry.SentryId(feedback.EventId.Value));
        _logger.LogDebug("Captured user feedback with ID {FeedbackId} for event {EventId}", feedbackId, feedback.EventId);
        return new PluginSentryEventId(feedbackId);
    }

    /// <inheritdoc/>
    [Obsolete("Use CaptureFeedback instead. CaptureUserFeedback was removed in Sentry SDK 6.0.0.")]
    public PluginSentryEventId CaptureUserFeedback(PluginSentryEventId eventId, string? email, string comments, string? name = null)
    {
        return CaptureFeedback(eventId, name ?? string.Empty, email ?? string.Empty, comments);
    }

    #endregion

    #region ICronMonitor

    /// <inheritdoc/>
    public string CheckInProgress(string monitorSlug)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
#endif

        var checkInId = Sentry.SentrySdk.CaptureCheckIn(monitorSlug, Sentry.CheckInStatus.InProgress);
        _logger.LogDebug("Check-in started for monitor {MonitorSlug} with ID {CheckInId}", monitorSlug, checkInId);
        return checkInId.ToString();
    }

    /// <inheritdoc/>
    public void CheckInOk(string monitorSlug, string? checkInId = null)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
#endif

        Sentry.SentryId? id = string.IsNullOrEmpty(checkInId) ? null : Sentry.SentryId.Parse(checkInId!);
        Sentry.SentrySdk.CaptureCheckIn(monitorSlug, Sentry.CheckInStatus.Ok, id);
        _logger.LogDebug("Check-in complete for monitor {MonitorSlug}", monitorSlug);
    }

    /// <inheritdoc/>
    public void CheckInError(string monitorSlug, string? checkInId = null)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
#endif

        Sentry.SentryId? id = string.IsNullOrEmpty(checkInId) ? null : Sentry.SentryId.Parse(checkInId!);
        Sentry.SentrySdk.CaptureCheckIn(monitorSlug, Sentry.CheckInStatus.Error, id);
        _logger.LogDebug("Check-in failed for monitor {MonitorSlug}", monitorSlug);
    }

    /// <inheritdoc/>
    public void ExecuteJob(string monitorSlug, Action job)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
        ArgumentNullException.ThrowIfNull(job);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(job);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
        ArgumentNullException.ThrowIfNull(job);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(job);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
        ArgumentNullException.ThrowIfNull(job);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(job);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);
        ArgumentNullException.ThrowIfNull(job);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(monitorSlug);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(job);
#endif

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
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(key);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(value);
#endif
        _hub.ConfigureScope(scope => scope.SetTag(key, value));
    }

    /// <inheritdoc/>
    public void SetExtra(string key, object? value)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(key);
#endif
        _hub.ConfigureScope(scope => scope.SetExtra(key, value));
    }

    /// <inheritdoc/>
    public void SetContext(string key, object value)
    {
#if NET5_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
#else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNullOrEmpty(key);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(value);
#endif
        _hub.ConfigureScope(scope => scope.Contexts[key] = value);
    }

    #endregion
}
