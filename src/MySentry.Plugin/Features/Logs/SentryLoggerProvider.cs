using Microsoft.Extensions.Logging;
using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Logs;

/// <summary>
/// Sentry logging provider that integrates with Microsoft.Extensions.Logging.
/// </summary>
public sealed class SentryLoggerProvider : ILoggerProvider
{
    private readonly ISentryPlugin _plugin;
    private readonly LogLevel _minimumLevel;

    /// <summary>
    /// Creates a new Sentry logger provider.
    /// </summary>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="minimumLevel">The minimum log level to capture.</param>
    public SentryLoggerProvider(ISentryPlugin plugin, LogLevel minimumLevel = LogLevel.Warning)
    {
        _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        _minimumLevel = minimumLevel;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new SentryLogger(_plugin, categoryName, _minimumLevel);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Sentry logger that captures logs as breadcrumbs and errors as events.
/// </summary>
internal sealed class SentryLogger : ILogger
{
    private readonly ISentryPlugin _plugin;
    private readonly string _categoryName;
    private readonly LogLevel _minimumLevel;

    public SentryLogger(ISentryPlugin plugin, string categoryName, LogLevel minimumLevel)
    {
        _plugin = plugin;
        _categoryName = categoryName;
        _minimumLevel = minimumLevel;
    }

#if NETCOREAPP
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }
#else
    public IDisposable BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }
#endif

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minimumLevel;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        // Capture exceptions as Sentry events
        if (exception is not null && logLevel >= LogLevel.Error)
        {
            _plugin.CaptureException(exception);
        }

        // Capture log messages as breadcrumbs
        var breadcrumbLevel = MapToBreadcrumbLevel(logLevel);
        _plugin.AddBreadcrumb(
            message,
            _categoryName,
            "log",
            new Dictionary<string, string>
            {
                ["eventId"] = eventId.ToString(),
                ["logLevel"] = logLevel.ToString()
            },
            breadcrumbLevel);
    }

    private static PluginBreadcrumbLevel MapToBreadcrumbLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => PluginBreadcrumbLevel.Debug,
            LogLevel.Debug => PluginBreadcrumbLevel.Debug,
            LogLevel.Information => PluginBreadcrumbLevel.Info,
            LogLevel.Warning => PluginBreadcrumbLevel.Warning,
            LogLevel.Error => PluginBreadcrumbLevel.Error,
            LogLevel.Critical => PluginBreadcrumbLevel.Critical,
            _ => PluginBreadcrumbLevel.Info
        };
    }
}

/// <summary>
/// Null scope implementation for ILogger.BeginScope.
/// </summary>
internal sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new();

    private NullScope()
    {
    }

    public void Dispose()
    {
    }
}
