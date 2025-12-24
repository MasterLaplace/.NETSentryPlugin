using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Core;

/// <summary>
/// Wraps a Sentry span to provide a clean abstraction layer.
/// </summary>
internal sealed class SpanTrackerWrapper : ISpanTracker
{
    private readonly Sentry.ISpan _span;
    private bool _isFinished;

    public SpanTrackerWrapper(Sentry.ISpan span)
    {
        _span = span;
    }

    public string TraceId => _span.TraceId.ToString();

    public string SpanId => _span.SpanId.ToString();

    public string? ParentSpanId => _span.ParentSpanId?.ToString();

    public string Operation => _span.Operation;

    public string? Description => _span.Description;

    public bool IsFinished => _isFinished;

    public ISpanTracker StartChild(string operation, string? description = null)
    {
        var child = _span.StartChild(operation, description);
        return new SpanTrackerWrapper(child);
    }

    public ISpanTracker SetTag(string key, string value)
    {
        _span.SetTag(key, value);
        return this;
    }

    public ISpanTracker SetExtra(string key, object? value)
    {
        _span.SetExtra(key, value);
        return this;
    }

    public void SetStatus(PluginSpanStatus status)
    {
        _span.Status = MapSpanStatus(status);
    }

    public void Finish()
    {
        if (_isFinished)
        {
            return;
        }

        _span.Finish();
        _isFinished = true;
    }

    public void Finish(PluginSpanStatus status)
    {
        if (_isFinished)
        {
            return;
        }

        _span.Finish(MapSpanStatus(status));
        _isFinished = true;
    }

    public void Finish(Exception exception)
    {
        if (_isFinished)
        {
            return;
        }

        _span.Finish(exception);
        _isFinished = true;
    }

    public void Dispose()
    {
        Finish();
    }

    internal static Sentry.SpanStatus MapSpanStatus(PluginSpanStatus status) => status switch
    {
        PluginSpanStatus.Ok => Sentry.SpanStatus.Ok,
        PluginSpanStatus.Cancelled => Sentry.SpanStatus.Cancelled,
        PluginSpanStatus.Unknown => Sentry.SpanStatus.UnknownError,
        PluginSpanStatus.InvalidArgument => Sentry.SpanStatus.InvalidArgument,
        PluginSpanStatus.DeadlineExceeded => Sentry.SpanStatus.DeadlineExceeded,
        PluginSpanStatus.NotFound => Sentry.SpanStatus.NotFound,
        PluginSpanStatus.AlreadyExists => Sentry.SpanStatus.AlreadyExists,
        PluginSpanStatus.PermissionDenied => Sentry.SpanStatus.PermissionDenied,
        PluginSpanStatus.ResourceExhausted => Sentry.SpanStatus.ResourceExhausted,
        PluginSpanStatus.FailedPrecondition => Sentry.SpanStatus.FailedPrecondition,
        PluginSpanStatus.Aborted => Sentry.SpanStatus.Aborted,
        PluginSpanStatus.OutOfRange => Sentry.SpanStatus.OutOfRange,
        PluginSpanStatus.Unimplemented => Sentry.SpanStatus.Unimplemented,
        PluginSpanStatus.InternalError => Sentry.SpanStatus.InternalError,
        PluginSpanStatus.Unavailable => Sentry.SpanStatus.Unavailable,
        PluginSpanStatus.DataLoss => Sentry.SpanStatus.DataLoss,
        PluginSpanStatus.Unauthenticated => Sentry.SpanStatus.Unauthenticated,
        _ => Sentry.SpanStatus.UnknownError
    };
}
