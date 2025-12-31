using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.UserFeedback;

/// <summary>
/// Handles user feedback capture and submission.
/// </summary>
public sealed class FeedbackHandler
{
    private readonly IUserFeedbackCapture _feedbackCapture;

    /// <summary>
    /// Creates a new feedback handler.
    /// </summary>
    /// <param name="feedbackCapture">The feedback capture service.</param>
    public FeedbackHandler(IUserFeedbackCapture feedbackCapture)
    {
        _feedbackCapture = feedbackCapture;
    }

    /// <summary>
    /// Submits user feedback.
    /// </summary>
    /// <param name="request">The feedback request.</param>
    /// <returns>The result of the submission.</returns>
    public FeedbackResult Submit(FeedbackRequest request)
    {
    #if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
    #else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(request);
    #endif

        if (string.IsNullOrWhiteSpace(request.Comments))
        {
            return FeedbackResult.Failed("Comments are required.");
        }

        var eventId = request.EventId ?? PluginSentryEventId.Empty;

        _feedbackCapture.CaptureFeedback(
            eventId,
            request.Name ?? string.Empty,
            request.Email ?? string.Empty,
            request.Comments);

        return FeedbackResult.Ok(eventId);
    }

    /// <summary>
    /// Submits user feedback with associated exception.
    /// </summary>
    /// <param name="request">The feedback request.</param>
    /// <param name="exception">The associated exception.</param>
    /// <param name="errorCapture">The error capture service.</param>
    /// <returns>The result of the submission.</returns>
    public FeedbackResult SubmitWithException(
        FeedbackRequest request,
        Exception exception,
        IErrorCapture errorCapture)
    {
    #if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(errorCapture);
    #else
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(request);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(exception);
        MySentry.Plugin.NetFrameworkPolyfills.ThrowIfNull(errorCapture);
    #endif

        if (string.IsNullOrWhiteSpace(request.Comments))
        {
            return FeedbackResult.Failed("Comments are required.");
        }

        // First capture the exception
        var eventId = errorCapture.CaptureException(exception);

        // Then attach the feedback
        _feedbackCapture.CaptureFeedback(
            eventId,
            request.Name ?? string.Empty,
            request.Email ?? string.Empty,
            request.Comments);

        return FeedbackResult.Ok(eventId);
    }
}
