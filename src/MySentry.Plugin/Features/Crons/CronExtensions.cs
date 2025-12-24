using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Crons;

/// <summary>
/// Extension methods for cron monitoring.
/// </summary>
public static class CronExtensions
{
    /// <summary>
    /// Creates and starts a cron job monitor.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <returns>A cron job monitor for tracking the job.</returns>
    public static CronJobMonitor MonitorCronJob(this ICronMonitor cronMonitor, string monitorSlug)
    {
        return CronJobMonitor.Start(cronMonitor, monitorSlug);
    }

    /// <summary>
    /// Creates and starts a cron job monitor with configuration.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="config">The cron job configuration.</param>
    /// <returns>A cron job monitor for tracking the job.</returns>
    public static CronJobMonitor MonitorCronJob(this ICronMonitor cronMonitor, CronJobConfig config)
    {
        return CronJobMonitor.Start(cronMonitor, config);
    }

    /// <summary>
    /// Executes an action within a monitored cron job context.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <param name="action">The action to execute.</param>
    public static void WithCronMonitoring(
        this ICronMonitor cronMonitor,
        string monitorSlug,
        Action action)
    {
        using var monitor = cronMonitor.MonitorCronJob(monitorSlug);
        monitor.Execute(action);
    }

    /// <summary>
    /// Executes a function within a monitored cron job context.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public static T WithCronMonitoring<T>(
        this ICronMonitor cronMonitor,
        string monitorSlug,
        Func<T> func)
    {
        using var monitor = cronMonitor.MonitorCronJob(monitorSlug);
        return monitor.Execute(func);
    }

    /// <summary>
    /// Executes an async function within a monitored cron job context.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <param name="func">The async function to execute.</param>
    public static async Task WithCronMonitoringAsync(
        this ICronMonitor cronMonitor,
        string monitorSlug,
        Func<Task> func)
    {
        await using var monitor = cronMonitor.MonitorCronJob(monitorSlug);
        await monitor.ExecuteAsync(func);
    }

    /// <summary>
    /// Executes an async function within a monitored cron job context.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <param name="func">The async function to execute.</param>
    /// <returns>The result of the function.</returns>
    public static async Task<T> WithCronMonitoringAsync<T>(
        this ICronMonitor cronMonitor,
        string monitorSlug,
        Func<Task<T>> func)
    {
        await using var monitor = cronMonitor.MonitorCronJob(monitorSlug);
        return await monitor.ExecuteAsync(func);
    }

    /// <summary>
    /// Sends a simple check-in for a cron job.
    /// </summary>
    /// <param name="cronMonitor">The cron monitor.</param>
    /// <param name="monitorSlug">The monitor slug.</param>
    /// <param name="status">The check-in status.</param>
    public static void CronCheckIn(
        this ICronMonitor cronMonitor,
        string monitorSlug,
        CheckInStatus status)
    {
        switch (status)
        {
            case CheckInStatus.InProgress:
                cronMonitor.CheckInProgress(monitorSlug);
                break;
            case CheckInStatus.Ok:
                cronMonitor.CheckInOk(monitorSlug);
                break;
            case CheckInStatus.Error:
                cronMonitor.CheckInError(monitorSlug);
                break;
        }
    }
}
