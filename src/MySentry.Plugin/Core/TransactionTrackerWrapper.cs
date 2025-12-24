using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Core;

/// <summary>
/// Wraps a Sentry transaction to provide a clean abstraction layer.
/// </summary>
internal sealed class TransactionTrackerWrapper : ITransactionTracker
{
    private readonly Sentry.ITransactionTracer _transaction;
    private bool _isFinished;

    public TransactionTrackerWrapper(Sentry.ITransactionTracer transaction)
    {
        _transaction = transaction;
    }

    public string TraceId => _transaction.TraceId.ToString();

    public string SpanId => _transaction.SpanId.ToString();

    public string Name => _transaction.Name;

    public string Operation => _transaction.Operation;

    public bool IsFinished => _isFinished;

    public ISpanTracker StartChild(string operation, string? description = null)
    {
        var child = _transaction.StartChild(operation, description);
        return new SpanTrackerWrapper(child);
    }

    public ITransactionTracker SetTag(string key, string value)
    {
        _transaction.SetTag(key, value);
        return this;
    }

    public ITransactionTracker SetExtra(string key, object? value)
    {
        _transaction.SetExtra(key, value);
        return this;
    }

    public ITransactionTracker SetHttpStatus(int statusCode)
    {
        _transaction.SetExtra("http.status_code", statusCode);

        var status = statusCode switch
        {
            >= 200 and < 300 => Sentry.SpanStatus.Ok,
            400 => Sentry.SpanStatus.InvalidArgument,
            401 => Sentry.SpanStatus.Unauthenticated,
            403 => Sentry.SpanStatus.PermissionDenied,
            404 => Sentry.SpanStatus.NotFound,
            409 => Sentry.SpanStatus.AlreadyExists,
            429 => Sentry.SpanStatus.ResourceExhausted,
            499 => Sentry.SpanStatus.Cancelled,
            501 => Sentry.SpanStatus.Unimplemented,
            503 => Sentry.SpanStatus.Unavailable,
            504 => Sentry.SpanStatus.DeadlineExceeded,
            >= 500 and < 600 => Sentry.SpanStatus.InternalError,
            _ => Sentry.SpanStatus.UnknownError
        };

        _transaction.Status = status;
        return this;
    }

    public void SetStatus(PluginSpanStatus status)
    {
        _transaction.Status = SpanTrackerWrapper.MapSpanStatus(status);
    }

    public void Finish()
    {
        if (_isFinished)
        {
            return;
        }

        _transaction.Finish();
        _isFinished = true;
    }

    public void Finish(PluginSpanStatus status)
    {
        if (_isFinished)
        {
            return;
        }

        _transaction.Finish(SpanTrackerWrapper.MapSpanStatus(status));
        _isFinished = true;
    }

    public void Finish(Exception exception)
    {
        if (_isFinished)
        {
            return;
        }

        _transaction.Finish(exception);
        _isFinished = true;
    }

    public void Dispose()
    {
        Finish();
    }
}
