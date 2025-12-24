namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides performance monitoring capabilities including transactions and spans.
/// Enables distributed tracing and performance insights across your application.
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Starts a new transaction for tracking a complete operation.
    /// </summary>
    /// <param name="name">The name of the transaction (e.g., "ProcessOrder").</param>
    /// <param name="operation">The operation type (e.g., "task", "http.request", "db.query").</param>
    /// <returns>A transaction tracker that should be disposed when the operation completes.</returns>
    ITransactionTracker StartTransaction(string name, string operation);

    /// <summary>
    /// Starts a new transaction with custom options.
    /// </summary>
    /// <param name="name">The name of the transaction.</param>
    /// <param name="operation">The operation type.</param>
    /// <param name="configure">Action to configure transaction options.</param>
    /// <returns>A transaction tracker that should be disposed when the operation completes.</returns>
    ITransactionTracker StartTransaction(string name, string operation, Action<TransactionOptions> configure);

    /// <summary>
    /// Gets the currently active span, if any.
    /// </summary>
    /// <returns>The current span, or null if no span is active.</returns>
    ISpanTracker? GetCurrentSpan();

    /// <summary>
    /// Gets the currently active transaction, if any.
    /// </summary>
    /// <returns>The current transaction, or null if no transaction is active.</returns>
    ITransactionTracker? GetCurrentTransaction();

    /// <summary>
    /// Creates a child span under the current span or transaction.
    /// </summary>
    /// <param name="operation">The operation type for the span.</param>
    /// <param name="description">A description of what the span represents.</param>
    /// <returns>A span tracker, or null if no parent span/transaction exists.</returns>
    ISpanTracker? StartSpan(string operation, string? description = null);
}
