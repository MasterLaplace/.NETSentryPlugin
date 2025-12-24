using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Crons;

/// <summary>
/// Provides cron job monitoring functionality with automatic check-in management.
/// </summary>
public sealed class CronJobMonitor : IDisposable, IAsyncDisposable
{
    private readonly ICronMonitor _cronMonitor;
    private readonly string _monitorSlug;
    private readonly string? _checkInId;
    private bool _isCompleted;
    private bool _disposed;

    private CronJobMonitor(ICronMonitor cronMonitor, string monitorSlug, string? checkInId)
    {
        _cronMonitor = cronMonitor;
        _monitorSlug = monitorSlug;
        _checkInId = checkInId;
    }

    /// <summary>
    /// Creates a new cron job monitor and starts tracking.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor service.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <returns>A new cron job monitor.</returns>
    public static CronJobMonitor Start(ICronMonitor cronMonitor, string monitorSlug)
    {
        ArgumentNullException.ThrowIfNull(cronMonitor);
        ArgumentException.ThrowIfNullOrEmpty(monitorSlug);

        var checkInId = cronMonitor.CheckInProgress(monitorSlug);
        return new CronJobMonitor(cronMonitor, monitorSlug, checkInId);
    }

    /// <summary>
    /// Creates a new cron job monitor with configuration and starts tracking.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor service.</param>
    /// <param name="config">The cron job configuration.</param>
    /// <returns>A new cron job monitor.</returns>
    public static CronJobMonitor Start(ICronMonitor cronMonitor, CronJobConfig config)
    {
        ArgumentNullException.ThrowIfNull(cronMonitor);
        ArgumentNullException.ThrowIfNull(config);

        var checkInId = cronMonitor.CheckInProgress(config.MonitorSlug);
        return new CronJobMonitor(cronMonitor, config.MonitorSlug, checkInId);
    }

    /// <summary>
    /// Marks the cron job as successfully completed.
    /// </summary>
    public void Complete()
    {
        EnsureNotCompleted();
        _cronMonitor.CheckInOk(_monitorSlug, _checkInId);
        _isCompleted = true;
    }

    /// <summary>
    /// Marks the cron job as failed.
    /// </summary>
    public void Fail()
    {
        EnsureNotCompleted();
        _cronMonitor.CheckInError(_monitorSlug, _checkInId);
        _isCompleted = true;
    }

    /// <summary>
    /// Executes the specified action within the cron monitor context.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Execute(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
            Complete();
        }
        catch
        {
            Fail();
            throw;
        }
    }

    /// <summary>
    /// Executes the specified function within the cron monitor context.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public T Execute<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            var result = func();
            Complete();
            return result;
        }
        catch
        {
            Fail();
            throw;
        }
    }

    /// <summary>
    /// Executes the specified async function within the cron monitor context.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    public async Task ExecuteAsync(Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            await func().ConfigureAwait(false);
            Complete();
        }
        catch
        {
            Fail();
            throw;
        }
    }

    /// <summary>
    /// Executes the specified async function within the cron monitor context.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <returns>The result of the function.</returns>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            var result = await func().ConfigureAwait(false);
            Complete();
            return result;
        }
        catch
        {
            Fail();
            throw;
        }
    }

    private void EnsureNotCompleted()
    {
        if (_isCompleted)
        {
            throw new InvalidOperationException("Cron job has already been completed.");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed && !_isCompleted)
        {
            // If not explicitly completed, mark as failed
            try { Fail(); } catch { /* Ignore dispose errors */ }
        }
        _disposed = true;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
