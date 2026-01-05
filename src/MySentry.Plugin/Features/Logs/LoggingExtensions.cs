using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Logs;

/// <summary>
/// Extension methods for integrating Sentry with Microsoft.Extensions.Logging.
/// </summary>
public static class LoggingExtensions
{
#if ASPNETCORE
    /// <summary>
    /// Adds Sentry as a logging provider.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="minimumLevel">The minimum log level to capture.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddSentry(this ILoggingBuilder builder, LogLevel minimumLevel = LogLevel.Warning)
    {
        builder.Services.AddSingleton<ILoggerProvider>(provider =>
        {
            var plugin = provider.GetRequiredService<ISentryPlugin>();
            return new SentryLoggerProvider(plugin, minimumLevel);
        });

        return builder;
    }
#endif

    /// <summary>
    /// Logs a message and adds it as a breadcrumb.
    /// </summary>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="category">The category.</param>
    public static void Log(this ISentryPlugin plugin, LogLevel level, string message, string? category = null)
    {
        var breadcrumbLevel = level switch
        {
            LogLevel.Trace => PluginBreadcrumbLevel.Debug,
            LogLevel.Debug => PluginBreadcrumbLevel.Debug,
            LogLevel.Information => PluginBreadcrumbLevel.Info,
            LogLevel.Warning => PluginBreadcrumbLevel.Warning,
            LogLevel.Error => PluginBreadcrumbLevel.Error,
            LogLevel.Critical => PluginBreadcrumbLevel.Fatal,
            _ => PluginBreadcrumbLevel.Info
        };

        plugin.AddBreadcrumb(message, category ?? "log", "log", breadcrumbLevel);
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public static void LogDebug(this ISentryPlugin plugin, string message, string? category = null)
    {
        plugin.Log(LogLevel.Debug, message, category);
    }

    /// <summary>
    /// Logs an information message.
    /// </summary>
    public static void LogInfo(this ISentryPlugin plugin, string message, string? category = null)
    {
        plugin.Log(LogLevel.Information, message, category);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void LogWarning(this ISentryPlugin plugin, string message, string? category = null)
    {
        plugin.Log(LogLevel.Warning, message, category);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void LogError(this ISentryPlugin plugin, string message, string? category = null)
    {
        plugin.Log(LogLevel.Error, message, category);
    }

    /// <summary>
    /// Logs a critical message.
    /// </summary>
    public static void LogCritical(this ISentryPlugin plugin, string message, string? category = null)
    {
        plugin.Log(LogLevel.Critical, message, category);
    }
}
