#if ASPNET_CLASSIC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Configuration;
using MySentry.Plugin.Core;
using MySentry.Plugin.Enrichers;
using Sentry;
using Sentry.AspNet;

namespace MySentry.Plugin.Extensions;

/// <summary>
/// Extension methods for configuring MySentry in ASP.NET Classic applications.
/// </summary>
public static class AspNetClassicExtensions
{
    private static IServiceProvider? _serviceProvider;
    private static SentryPluginOptions? _options;
    private static IDisposable? _sentryInstance;

    /// <summary>
    /// Initializes MySentry for ASP.NET Classic applications.
    /// Call this in Global.asax Application_Start.
    /// </summary>
    /// <param name="configure">Action to configure the plugin.</param>
    /// <returns>The Sentry plugin instance.</returns>
    public static ISentryPlugin InitMySentry(Action<SentryPluginBuilder> configure)
    {
        var builder = new SentryPluginBuilder();
        configure(builder);
        builder.ApplyConfiguration();
        _options = builder.Options;

        // Initialize Sentry SDK
        _sentryInstance = SentrySdk.Init(options =>
        {
            ConfigureSentryOptions(options, _options);
        });

        // Build service provider
        var services = new ServiceCollection();
        ConfigureServices(services, builder);
        _serviceProvider = services.BuildServiceProvider();

        return _serviceProvider.GetRequiredService<ISentryPlugin>();
    }

    /// <summary>
    /// Gets the Sentry plugin instance.
    /// </summary>
    public static ISentryPlugin GetPlugin()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException(
                "MySentry has not been initialized. Call InitMySentry in Application_Start first.");
        }

        return _serviceProvider.GetRequiredService<ISentryPlugin>();
    }

    /// <summary>
    /// Starts a transaction for the current HTTP request.
    /// Call this in Application_BeginRequest.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public static void StartRequestTransaction(HttpContext context)
    {
        if (context is null || _options is null || !_options.Tracing.Enabled)
        {
            return;
        }

        var request = context.Request;
        var transactionName = $"{request.HttpMethod} {request.Url?.AbsolutePath ?? "/"}";

        var transaction = SentrySdk.StartTransaction(transactionName, "http.server");
        SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

        context.Items["__mysentry.transaction"] = transaction;
        context.Items["__mysentry.stopwatch"] = Stopwatch.StartNew();
    }

    /// <summary>
    /// Finishes the transaction for the current HTTP request.
    /// Call this in Application_EndRequest.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public static void FinishRequestTransaction(HttpContext context)
    {
        if (context is null)
        {
            return;
        }

        var stopwatch = context.Items["__mysentry.stopwatch"] as Stopwatch;
        stopwatch?.Stop();

        if (context.Items["__mysentry.transaction"] is ITransactionTracer transaction)
        {
            if (stopwatch is not null)
            {
                transaction.SetExtra("duration_ms", stopwatch.ElapsedMilliseconds);
            }

            // Set HTTP status using the Status property (compatible with all .NET versions)
            var statusCode = context.Response.StatusCode;
            transaction.SetExtra("http.status_code", statusCode);
            transaction.Status = MapHttpStatusToSpanStatus(statusCode);
            transaction.Finish();
        }

        context.Items.Remove("__mysentry.transaction");
        context.Items.Remove("__mysentry.stopwatch");
    }

    /// <summary>
    /// Captures an exception and sends it to Sentry.
    /// Call this in Application_Error.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    public static void CaptureException(Exception exception)
    {
        if (_serviceProvider is null)
        {
            SentrySdk.CaptureException(exception);
            return;
        }

        var plugin = _serviceProvider.GetRequiredService<ISentryPlugin>();
        plugin.CaptureException(exception);
    }

    /// <summary>
    /// Captures an exception with HTTP context.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="context">The HTTP context.</param>
    public static void CaptureException(Exception exception, HttpContext context)
    {
        if (_serviceProvider is null)
        {
            SentrySdk.CaptureException(exception);
            return;
        }

        var plugin = _serviceProvider.GetRequiredService<ISentryPlugin>();
        plugin.CaptureException(exception, scope =>
        {
            if (context?.Request is not null)
            {
                scope.SetTag("http.method", context.Request.HttpMethod);
                scope.SetTag("http.url", context.Request.Url?.AbsolutePath ?? "/");
                scope.SetExtra("http.user_agent", context.Request.UserAgent);
            }
        });
    }

    /// <summary>
    /// Sets the current user context.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="email">The user email (optional).</param>
    /// <param name="username">The username (optional).</param>
    public static void SetUser(string userId, string? email = null, string? username = null)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.User = new Sentry.SentryUser
            {
                Id = userId,
                Email = email,
                Username = username
            };
        });
    }

    /// <summary>
    /// Clears the current user context.
    /// </summary>
    public static void ClearUser()
    {
        SentrySdk.ConfigureScope(scope => scope.User = null!);
    }

    /// <summary>
    /// Releases all resources. Call this in Application_End.
    /// </summary>
    public static void Release()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _sentryInstance?.Dispose();
        _serviceProvider = null;
        _sentryInstance = null;
    }

    private static void ConfigureServices(IServiceCollection services, SentryPluginBuilder builder)
    {
        services.AddSingleton(_options!);
        services.AddSingleton<IOptions<SentryPluginOptions>>(sp =>
            Options.Create(sp.GetRequiredService<SentryPluginOptions>()));

        // Register a factory that provides access to the current hub
        // For Sentry SDK 4.x, we access the hub through reflection since CurrentHub is not public
        services.AddSingleton<IHub>(sp =>
        {
            // Try to access the hub through reflection
            var hubProperty = typeof(SentrySdk).GetProperty("CurrentHub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                           ?? typeof(SentrySdk).GetProperty("CurrentHub", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (hubProperty != null)
            {
                var hub = hubProperty.GetValue(null) as IHub;
                if (hub != null) return hub;
            }

            // Try field access
            var hubField = typeof(SentrySdk).GetField("CurrentHub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                        ?? typeof(SentrySdk).GetField("CurrentHub", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (hubField != null)
            {
                var hub = hubField.GetValue(null) as IHub;
                if (hub != null) return hub;
            }

            // Fallback: create a simple wrapper that delegates to SentrySdk static methods
            return new SentrySdkHubAdapter();
        });

        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddConsole();
        });

        // Register core services
        services.AddSingleton<ISentryPlugin, SentryPlugin>();
        services.AddSingleton<IErrorCapture>(sp => sp.GetRequiredService<ISentryPlugin>());
        services.AddSingleton<IPerformanceMonitor>(sp => sp.GetRequiredService<ISentryPlugin>());
        services.AddSingleton<IBreadcrumbTracker>(sp => sp.GetRequiredService<ISentryPlugin>());
        services.AddSingleton<IUserContextProvider>(sp => sp.GetRequiredService<ISentryPlugin>());
        services.AddSingleton<IScopeManager>(sp => sp.GetRequiredService<ISentryPlugin>());
        services.AddSingleton<IUserFeedbackCapture>(sp =>
            (IUserFeedbackCapture)sp.GetRequiredService<ISentryPlugin>());
        services.AddSingleton<ICronMonitor>(sp =>
            (ICronMonitor)sp.GetRequiredService<ISentryPlugin>());

        // Register enrichers
        foreach (var enricherType in builder.EnricherTypes)
        {
            services.AddTransient(typeof(IEventEnricher), enricherType);
        }

        foreach (var enricherInstance in builder.EnricherInstances)
        {
            services.AddSingleton(typeof(IEventEnricher), enricherInstance);
        }

        services.AddTransient<IEventEnricher, EnvironmentEnricher>();
        // Note: UserEnricher is ASP.NET Core only, not available for ASP.NET Classic
    }

    private static void ConfigureSentryOptions(SentryOptions options, SentryPluginOptions pluginOptions)
    {
        options.Dsn = pluginOptions.Dsn;
        options.Debug = pluginOptions.Debug;
        options.Environment = pluginOptions.Environment;
        options.Release = pluginOptions.Release;
        options.SendDefaultPii = pluginOptions.SendDefaultPii;
        options.SampleRate = (float?)pluginOptions.SampleRate;
        options.MaxBreadcrumbs = pluginOptions.MaxBreadcrumbs;
        options.AttachStacktrace = true;
        options.AutoSessionTracking = true;

        if (!string.IsNullOrEmpty(pluginOptions.ServerName))
        {
            options.ServerName = pluginOptions.ServerName;
        }

        if (pluginOptions.Tracing.Enabled)
        {
            options.TracesSampleRate = pluginOptions.Tracing.SampleRate;
            options.TracesSampler = context =>
            {
                var name = context.TransactionContext.Name ?? string.Empty;
                foreach (var pattern in pluginOptions.Tracing.IgnoreUrls)
                {
                    if (MatchesPattern(name, pattern))
                    {
                        return 0.0;
                    }
                }
                return pluginOptions.Tracing.SampleRate;
            };
        }

        // Add InApp patterns
        foreach (var pattern in pluginOptions.InAppInclude)
        {
            options.AddInAppInclude(pattern);
        }

        foreach (var pattern in pluginOptions.InAppExclude)
        {
            options.AddInAppExclude(pattern);
        }

        // Default tags
        foreach (var tag in pluginOptions.DefaultTags)
        {
            options.DefaultTags.Add(tag.Key, tag.Value);
        }
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (pattern.EndsWith("*"))
        {
            return value.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith("*"))
        {
            return value.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase);
        }

        return value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static Sentry.SpanStatus MapHttpStatusToSpanStatus(int statusCode)
    {
        if (statusCode >= 200 && statusCode < 300)
            return Sentry.SpanStatus.Ok;
        if (statusCode == 400)
            return Sentry.SpanStatus.InvalidArgument;
        if (statusCode == 401)
            return Sentry.SpanStatus.Unauthenticated;
        if (statusCode == 403)
            return Sentry.SpanStatus.PermissionDenied;
        if (statusCode == 404)
            return Sentry.SpanStatus.NotFound;
        if (statusCode == 409)
            return Sentry.SpanStatus.AlreadyExists;
        if (statusCode == 429)
            return Sentry.SpanStatus.ResourceExhausted;
        if (statusCode == 499)
            return Sentry.SpanStatus.Cancelled;
        if (statusCode >= 500 && statusCode < 600)
            return Sentry.SpanStatus.InternalError;
        if (statusCode == 501)
            return Sentry.SpanStatus.Unimplemented;
        if (statusCode == 503)
            return Sentry.SpanStatus.Unavailable;
        if (statusCode == 504)
            return Sentry.SpanStatus.DeadlineExceeded;

        return statusCode < 400 ? Sentry.SpanStatus.Ok : Sentry.SpanStatus.UnknownError;
    }
}

/// <summary>
/// HTTP Module for automatic MySentry integration in ASP.NET Classic.
/// Register in web.config under system.webServer/modules.
/// </summary>
public sealed class MySentryHttpModule : IHttpModule
{
    /// <inheritdoc/>
    public void Init(HttpApplication context)
    {
        context.BeginRequest += OnBeginRequest;
        context.EndRequest += OnEndRequest;
        context.Error += OnError;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Nothing to dispose
    }

    private static void OnBeginRequest(object sender, EventArgs e)
    {
        if (sender is HttpApplication app)
        {
            AspNetClassicExtensions.StartRequestTransaction(app.Context);
        }
    }

    private static void OnEndRequest(object sender, EventArgs e)
    {
        if (sender is HttpApplication app)
        {
            AspNetClassicExtensions.FinishRequestTransaction(app.Context);
        }
    }

    private static void OnError(object sender, EventArgs e)
    {
        if (sender is HttpApplication app)
        {
            var exception = app.Server.GetLastError();
            if (exception is not null)
            {
                AspNetClassicExtensions.CaptureException(exception, app.Context);
            }
        }
    }
}

/// <summary>
/// Adapter that provides IHub interface by delegating to SentrySdk static methods.
/// Used as fallback when the internal hub cannot be accessed via reflection.
/// </summary>
internal sealed class SentrySdkHubAdapter : IHub
{
    public bool IsEnabled => SentrySdk.IsEnabled;
    public SentryId LastEventId => SentrySdk.LastEventId;
#if NET8_0_OR_GREATER
    public IMetricAggregator Metrics => throw new NotSupportedException("Metrics not supported in this adapter");
#endif

    public SentryId CaptureEvent(SentryEvent evt, Scope? scope = null) => SentrySdk.CaptureEvent(evt);
    public SentryId CaptureEvent(SentryEvent evt, Action<Scope> configureScope)
    {
        SentrySdk.ConfigureScope(configureScope);
        return SentrySdk.CaptureEvent(evt);
    }
    public SentryId CaptureEvent(SentryEvent evt, Scope? scope, SentryHint? hint) => SentrySdk.CaptureEvent(evt);
    public SentryId CaptureEvent(SentryEvent evt, SentryHint? hint, Action<Scope> configureScope)
    {
        SentrySdk.ConfigureScope(configureScope);
        return SentrySdk.CaptureEvent(evt);
    }

    public void CaptureTransaction(SentryTransaction transaction) => SentrySdk.CaptureTransaction(transaction);
    public void CaptureTransaction(SentryTransaction transaction, Scope? scope, SentryHint? hint) => SentrySdk.CaptureTransaction(transaction);
    public void CaptureSession(SessionUpdate sessionUpdate) { /* Handled internally */ }

    public SentryId CaptureException(Exception exception, Scope? scope = null) => SentrySdk.CaptureException(exception);
    public SentryId CaptureException(Exception exception, Action<Scope> configureScope) => SentrySdk.CaptureException(exception, configureScope);

#if NET8_0_OR_GREATER
    [Obsolete("Use CaptureFeedback instead. CaptureUserFeedback was removed in Sentry SDK 6.0.0.")]
    public void CaptureUserFeedback(Sentry.UserFeedback userFeedback) => SentrySdk.CaptureFeedback(userFeedback.Comments, userFeedback.Email, userFeedback.Name, userFeedback.EventId);
#endif

    public void AddBreadcrumb(Breadcrumb breadcrumb, SentryHint? hint = null) =>
        SentrySdk.AddBreadcrumb(breadcrumb.Message ?? string.Empty, breadcrumb.Category, breadcrumb.Type, breadcrumb.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), breadcrumb.Level);

    public void AddBreadcrumb(string message, string? category = null, string? type = null, IDictionary<string, string>? data = null, Sentry.BreadcrumbLevel level = Sentry.BreadcrumbLevel.Info) =>
        SentrySdk.AddBreadcrumb(message, category, type, data, level);

    public void ConfigureScope(Action<Scope> configureScope) => SentrySdk.ConfigureScope(configureScope);
    public Task ConfigureScopeAsync(Func<Scope, Task> configureScope) => SentrySdk.ConfigureScopeAsync(configureScope);
    public IDisposable PushScope() => SentrySdk.PushScope();
    public IDisposable PushScope<TState>(TState state) => SentrySdk.PushScope(state);

    public void WithScope(Action<Scope> scopeCallback)
    {
        using (SentrySdk.PushScope())
        {
            SentrySdk.ConfigureScope(scopeCallback);
        }
    }
    public void BindClient(ISentryClient client) { /* Not applicable */ }
    public void BindException(Exception exception, ISpan span) { /* Not applicable */ }

    public ITransactionTracer StartTransaction(ITransactionContext context, IReadOnlyDictionary<string, object?> customSamplingContext) =>
        SentrySdk.StartTransaction(context, customSamplingContext);
    public ITransactionTracer StartTransaction(string name, string operation) => SentrySdk.StartTransaction(name, operation);

    public SentryTraceHeader? GetTraceHeader() => SentrySdk.GetTraceHeader();
    public BaggageHeader? GetBaggage() => SentrySdk.GetBaggage();

    public TransactionContext ContinueTrace(SentryTraceHeader? traceHeader, BaggageHeader? baggageHeader, string? name = null, string? operation = null) =>
        SentrySdk.ContinueTrace(traceHeader, baggageHeader, name, operation);
    public TransactionContext ContinueTrace(string? traceHeader, string? baggageHeader, string? name = null, string? operation = null) =>
        SentrySdk.ContinueTrace(traceHeader, baggageHeader, name, operation);

    public ISpan? GetSpan() => SentrySdk.GetSpan();

    public SentryId CaptureMessage(string message, Sentry.SentryLevel level = Sentry.SentryLevel.Info) => SentrySdk.CaptureMessage(message, level);
    public SentryId CaptureMessage(string message, Action<Scope> configureScope, Sentry.SentryLevel level = Sentry.SentryLevel.Info) =>
        SentrySdk.CaptureMessage(message, configureScope, level);

    public Task FlushAsync(TimeSpan timeout) => SentrySdk.FlushAsync(timeout);

    public void StartSession() => SentrySdk.StartSession();
    public void PauseSession() => SentrySdk.PauseSession();
    public void ResumeSession() => SentrySdk.ResumeSession();
    public void EndSession(SessionEndStatus status = SessionEndStatus.Exited) => SentrySdk.EndSession(status);

    public bool CaptureEnvelope(Sentry.Protocol.Envelopes.Envelope envelope) => false; // Not directly available via SentrySdk

    public SentryId CaptureCheckIn(string monitorSlug, CheckInStatus status, SentryId? checkInId = null, TimeSpan? duration = null, Scope? scope = null, Action<SentryMonitorOptions>? configureMonitorOptions = null) =>
        SentrySdk.CaptureCheckIn(monitorSlug, status, checkInId);

    // Newer Sentry IHub members (implemented as adapter delegates)
    public W3CTraceparentHeader? GetTraceparentHeader() => SentrySdk.GetTraceparentHeader();

    public bool IsSessionActive => SentrySdk.IsSessionActive;

    public SentryStructuredLogger Logger => throw new NotSupportedException("Structured logging not supported in this adapter");

    public SentryId CaptureFeedback(Sentry.SentryFeedback feedback, out Sentry.CaptureFeedbackResult result, Action<Scope>? configureScope = null, Sentry.SentryHint? hint = null)
    {
        if (configureScope != null) SentrySdk.ConfigureScope(configureScope);
        result = default;
        return SentryId.Empty;
    }

    public SentryId CaptureFeedback(Sentry.SentryFeedback feedback, out Sentry.CaptureFeedbackResult result, Scope? scope = null, Sentry.SentryHint? hint = null)
    {
        // Fallback implementation for ISentryClient API surface
        result = default;
        return SentryId.Empty;
    }

    public void ConfigureScope<TArg>(Action<Scope, TArg> action, TArg arg)
    {
        SentrySdk.ConfigureScope(scope => action(scope, arg));
    }

    public Task ConfigureScopeAsync<TArg>(Func<Scope, TArg, Task> action, TArg arg)
    {
        return SentrySdk.ConfigureScopeAsync(scope => action(scope, arg));
    }

    public void SetTag(string key, string value) => SentrySdk.ConfigureScope(s => s.SetTag(key, value));
    public void UnsetTag(string key) => SentrySdk.ConfigureScope(s => s.SetTag(key, string.Empty));
}
#endif
