namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Main entry point for all Sentry operations.
/// Provides a unified, testable interface for error tracking, performance monitoring, and observability.
/// </summary>
public interface ISentryPlugin : IErrorCapture, IPerformanceMonitor, IBreadcrumbTracker, IUserContextProvider, IScopeManager
{
    /// <summary>
    /// Gets a value indicating whether the Sentry SDK is initialized and operational.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the last event ID that was captured by Sentry.
    /// </summary>
    /// <returns>The last captured event ID, or an empty ID if no events have been captured.</returns>
    SentryEventId LastEventId { get; }

    /// <summary>
    /// Flushes all pending events to Sentry.
    /// Call this before application shutdown to ensure all events are transmitted.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the flush operation.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    Task FlushAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the error capture service for detailed error handling operations.
    /// </summary>
    IErrorCapture Errors { get; }

    /// <summary>
    /// Gets the performance monitoring service for tracing and transaction operations.
    /// </summary>
    IPerformanceMonitor Performance { get; }

    /// <summary>
    /// Gets the breadcrumb tracking service for navigation trail operations.
    /// </summary>
    IBreadcrumbTracker Breadcrumbs { get; }

    /// <summary>
    /// Gets the user context provider for user identification operations.
    /// </summary>
    IUserContextProvider UserContext { get; }

    /// <summary>
    /// Gets the scope manager for context isolation operations.
    /// </summary>
    IScopeManager Scopes { get; }
}
