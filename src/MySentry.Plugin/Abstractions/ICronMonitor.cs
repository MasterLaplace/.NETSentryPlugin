namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides cron job monitoring capabilities.
/// Monitor scheduled jobs and receive alerts when they fail or don't run on time.
/// </summary>
public interface ICronMonitor
{
    /// <summary>
    /// Reports that a cron job is starting.
    /// </summary>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <returns>A check-in ID that should be used when reporting completion.</returns>
    string CheckInProgress(string monitorSlug);

    /// <summary>
    /// Reports that a cron job completed successfully.
    /// </summary>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <param name="checkInId">The check-in ID returned from <see cref="CheckInProgress"/>.</param>
    void CheckInOk(string monitorSlug, string? checkInId = null);

    /// <summary>
    /// Reports that a cron job failed.
    /// </summary>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <param name="checkInId">The check-in ID returned from <see cref="CheckInProgress"/>.</param>
    void CheckInError(string monitorSlug, string? checkInId = null);

    /// <summary>
    /// Executes a cron job with automatic check-in management.
    /// </summary>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <param name="job">The job to execute.</param>
    void ExecuteJob(string monitorSlug, Action job);

    /// <summary>
    /// Executes an async cron job with automatic check-in management.
    /// </summary>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <param name="job">The async job to execute.</param>
    /// <returns>A task representing the job execution.</returns>
    Task ExecuteJobAsync(string monitorSlug, Func<Task> job);

    /// <summary>
    /// Executes a cron job with automatic check-in management and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <param name="job">The job to execute.</param>
    /// <returns>The result of the job.</returns>
    T ExecuteJob<T>(string monitorSlug, Func<T> job);

    /// <summary>
    /// Executes an async cron job with automatic check-in management and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="monitorSlug">The unique identifier for the monitor.</param>
    /// <param name="job">The async job to execute.</param>
    /// <returns>A task representing the job execution with the result.</returns>
    Task<T> ExecuteJobAsync<T>(string monitorSlug, Func<Task<T>> job);
}
