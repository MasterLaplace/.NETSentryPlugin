using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Tracing;

/// <summary>
/// Provides extension methods for tracing operations.
/// </summary>
public static class TracingExtensions
{
    /// <summary>
    /// Executes an action within a new span.
    /// </summary>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="description">The span description.</param>
    /// <param name="action">The action to execute.</param>
    public static void WithSpan(
        this IPerformanceMonitor monitor,
        string operation,
        string? description,
        Action action)
    {
        using var span = monitor.StartSpan(operation, description);
        try
        {
            action();
            span?.Finish(PluginSpanStatus.Ok);
        }
        catch (Exception ex)
        {
            span?.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an async action within a new span.
    /// </summary>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="description">The span description.</param>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A task representing the execution.</returns>
    public static async Task WithSpanAsync(
        this IPerformanceMonitor monitor,
        string operation,
        string? description,
        Func<Task> action)
    {
        using var span = monitor.StartSpan(operation, description);
        try
        {
            await action().ConfigureAwait(false);
            span?.Finish(PluginSpanStatus.Ok);
        }
        catch (Exception ex)
        {
            span?.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes a function within a new span and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="description">The span description.</param>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public static T WithSpan<T>(
        this IPerformanceMonitor monitor,
        string operation,
        string? description,
        Func<T> func)
    {
        using var span = monitor.StartSpan(operation, description);
        try
        {
            var result = func();
            span?.Finish(PluginSpanStatus.Ok);
            return result;
        }
        catch (Exception ex)
        {
            span?.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an async function within a new span and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="description">The span description.</param>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A task containing the result.</returns>
    public static async Task<T> WithSpanAsync<T>(
        this IPerformanceMonitor monitor,
        string operation,
        string? description,
        Func<Task<T>> func)
    {
        using var span = monitor.StartSpan(operation, description);
        try
        {
            var result = await func().ConfigureAwait(false);
            span?.Finish(PluginSpanStatus.Ok);
            return result;
        }
        catch (Exception ex)
        {
            span?.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an action within a new transaction.
    /// </summary>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="name">The transaction name.</param>
    /// <param name="operation">The operation type.</param>
    /// <param name="action">The action to execute.</param>
    public static void WithTransaction(
        this IPerformanceMonitor monitor,
        string name,
        string operation,
        Action action)
    {
        using var transaction = monitor.StartTransaction(name, operation);
        try
        {
            action();
            transaction.Finish(PluginSpanStatus.Ok);
        }
        catch (Exception ex)
        {
            transaction.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an async action within a new transaction.
    /// </summary>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="name">The transaction name.</param>
    /// <param name="operation">The operation type.</param>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A task representing the execution.</returns>
    public static async Task WithTransactionAsync(
        this IPerformanceMonitor monitor,
        string name,
        string operation,
        Func<Task> action)
    {
        using var transaction = monitor.StartTransaction(name, operation);
        try
        {
            await action().ConfigureAwait(false);
            transaction.Finish(PluginSpanStatus.Ok);
        }
        catch (Exception ex)
        {
            transaction.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes a function within a new transaction and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="name">The transaction name.</param>
    /// <param name="operation">The operation type.</param>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public static T WithTransaction<T>(
        this IPerformanceMonitor monitor,
        string name,
        string operation,
        Func<T> func)
    {
        using var transaction = monitor.StartTransaction(name, operation);
        try
        {
            var result = func();
            transaction.Finish(PluginSpanStatus.Ok);
            return result;
        }
        catch (Exception ex)
        {
            transaction.Finish(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an async function within a new transaction and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="monitor">The performance monitor.</param>
    /// <param name="name">The transaction name.</param>
    /// <param name="operation">The operation type.</param>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A task containing the result.</returns>
    public static async Task<T> WithTransactionAsync<T>(
        this IPerformanceMonitor monitor,
        string name,
        string operation,
        Func<Task<T>> func)
    {
        using var transaction = monitor.StartTransaction(name, operation);
        try
        {
            var result = await func().ConfigureAwait(false);
            transaction.Finish(PluginSpanStatus.Ok);
            return result;
        }
        catch (Exception ex)
        {
            transaction.Finish(ex);
            throw;
        }
    }
}
