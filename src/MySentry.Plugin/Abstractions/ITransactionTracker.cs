namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Tracks a distributed transaction for performance monitoring.
/// A transaction represents a complete operation that may contain multiple spans.
/// </summary>
public interface ITransactionTracker : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this transaction.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Gets the unique identifier for this span within the trace.
    /// </summary>
    string SpanId { get; }

    /// <summary>
    /// Gets the name of this transaction.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the operation type of this transaction.
    /// </summary>
    string Operation { get; }

    /// <summary>
    /// Gets a value indicating whether this transaction has been finished.
    /// </summary>
    bool IsFinished { get; }

    /// <summary>
    /// Starts a child span within this transaction.
    /// </summary>
    /// <param name="operation">The operation type for the child span.</param>
    /// <param name="description">A description of what the span represents.</param>
    /// <returns>A span tracker for the child span.</returns>
    ISpanTracker StartChild(string operation, string? description = null);

    /// <summary>
    /// Sets a tag on this transaction.
    /// Tags are indexed and searchable in Sentry.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>This transaction tracker for fluent chaining.</returns>
    ITransactionTracker SetTag(string key, string value);

    /// <summary>
    /// Sets extra data on this transaction.
    /// Extra data is not indexed but provides additional context.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>This transaction tracker for fluent chaining.</returns>
    ITransactionTracker SetExtra(string key, object? value);

    /// <summary>
    /// Sets the HTTP status code for this transaction.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>This transaction tracker for fluent chaining.</returns>
    ITransactionTracker SetHttpStatus(int statusCode);

    /// <summary>
    /// Finishes this transaction with a successful status.
    /// </summary>
    void Finish();

    /// <summary>
    /// Finishes this transaction with the specified status.
    /// </summary>
    /// <param name="status">The final status of the transaction.</param>
    void Finish(SpanStatus status);

    /// <summary>
    /// Finishes this transaction with an exception status.
    /// </summary>
    /// <param name="exception">The exception that caused the transaction to fail.</param>
    void Finish(Exception exception);
}
