namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Tracks a span within a transaction for performance monitoring.
/// Spans represent individual operations within a larger transaction.
/// </summary>
public interface ISpanTracker : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for the trace this span belongs to.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Gets the unique identifier for this span.
    /// </summary>
    string SpanId { get; }

    /// <summary>
    /// Gets the parent span ID, if this span has a parent.
    /// </summary>
    string? ParentSpanId { get; }

    /// <summary>
    /// Gets the operation type of this span.
    /// </summary>
    string Operation { get; }

    /// <summary>
    /// Gets the description of this span.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets a value indicating whether this span has been finished.
    /// </summary>
    bool IsFinished { get; }

    /// <summary>
    /// Starts a child span within this span.
    /// </summary>
    /// <param name="operation">The operation type for the child span.</param>
    /// <param name="description">A description of what the span represents.</param>
    /// <returns>A span tracker for the child span.</returns>
    ISpanTracker StartChild(string operation, string? description = null);

    /// <summary>
    /// Sets a tag on this span.
    /// Tags are indexed and searchable in Sentry.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>This span tracker for fluent chaining.</returns>
    ISpanTracker SetTag(string key, string value);

    /// <summary>
    /// Sets extra data on this span.
    /// Extra data is not indexed but provides additional context.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>This span tracker for fluent chaining.</returns>
    ISpanTracker SetExtra(string key, object? value);

    /// <summary>
    /// Finishes this span with a successful status.
    /// </summary>
    void Finish();

    /// <summary>
    /// Finishes this span with the specified status.
    /// </summary>
    /// <param name="status">The final status of the span.</param>
    void Finish(SpanStatus status);

    /// <summary>
    /// Finishes this span with an exception status.
    /// </summary>
    /// <param name="exception">The exception that caused the span to fail.</param>
    void Finish(Exception exception);
}
