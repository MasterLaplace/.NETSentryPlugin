using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Profiling;

/// <summary>
/// Provides profiling utilities and helpers.
/// </summary>
public static class ProfilingExtensions
{
    /// <summary>
    /// Creates a profiled operation that measures execution time.
    /// </summary>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>A disposable profiled operation.</returns>
    public static ProfiledOperation Profile(
        this ISentryPlugin plugin,
        string operationName,
        string? description = null)
    {
        return new ProfiledOperation(plugin, operationName, description);
    }

    /// <summary>
    /// Executes and profiles an action.
    /// </summary>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="action">The action to execute.</param>
    public static void Profiled(
        this ISentryPlugin plugin,
        string operationName,
        Action action)
    {
        using var _ = plugin.Profile(operationName);
        action();
    }

    /// <summary>
    /// Executes and profiles a function.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result.</returns>
    public static T Profiled<T>(
        this ISentryPlugin plugin,
        string operationName,
        Func<T> func)
    {
        using var _ = plugin.Profile(operationName);
        return func();
    }

    /// <summary>
    /// Executes and profiles an async function.
    /// </summary>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="func">The async function to execute.</param>
    public static async Task ProfiledAsync(
        this ISentryPlugin plugin,
        string operationName,
        Func<Task> func)
    {
        using var _ = plugin.Profile(operationName);
        await func().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes and profiles an async function.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="plugin">The Sentry plugin.</param>
    /// <param name="operationName">The operation name.</param>
    /// <param name="func">The async function to execute.</param>
    /// <returns>The result.</returns>
    public static async Task<T> ProfiledAsync<T>(
        this ISentryPlugin plugin,
        string operationName,
        Func<Task<T>> func)
    {
        using var _ = plugin.Profile(operationName);
        return await func().ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a profiled operation with timing.
/// </summary>
public sealed class ProfiledOperation : IDisposable
{
    private readonly ISentryPlugin _plugin;
    private readonly string _operationName;
    private readonly string? _description;
    private readonly ISpanTracker? _span;
    private readonly DateTime _startTime;
    private bool _disposed;
    private PluginSpanStatus _status = PluginSpanStatus.Ok;

    /// <summary>
    /// Creates a new profiled operation.
    /// </summary>
    public ProfiledOperation(ISentryPlugin plugin, string operationName, string? description)
    {
        _plugin = plugin;
        _operationName = operationName;
        _description = description;
        _startTime = DateTime.UtcNow;

        // Try to create a span if there's an active transaction
        _span = plugin.StartSpan(operationName, description);
    }

    /// <summary>
    /// Gets the elapsed time since the operation started.
    /// </summary>
    public TimeSpan Elapsed => DateTime.UtcNow - _startTime;

    /// <summary>
    /// Marks the operation as successful.
    /// </summary>
    public void Success()
    {
        _status = PluginSpanStatus.Ok;
    }

    /// <summary>
    /// Marks the operation as failed.
    /// </summary>
    /// <param name="error">Optional error message.</param>
    public void Fail(string? error = null)
    {
        _status = PluginSpanStatus.InternalError;
        if (error is not null)
        {
            _span?.SetTag("error.message", error);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var duration = Elapsed;
        _span?.SetExtra("duration_ms", duration.TotalMilliseconds);
        _span?.Finish(_status);

        // Add breadcrumb for the profiled operation
        _plugin.AddBreadcrumb(
            $"Operation '{_operationName}' completed in {duration.TotalMilliseconds:F2}ms",
            "profiling",
            "profiling",
            new Dictionary<string, string>
            {
                ["operation"] = _operationName,
                ["duration_ms"] = duration.TotalMilliseconds.ToString("F2")
            },
            PluginBreadcrumbLevel.Debug);
    }
}
