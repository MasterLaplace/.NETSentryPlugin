using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.UserFeedback;

/// <summary>
/// Request model for user feedback submissions.
/// </summary>
public sealed class FeedbackRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the feedback comments.
    /// </summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the associated event ID.
    /// </summary>
    public SentryEventId? EventId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; set; }
}

/// <summary>
/// Result of a feedback submission.
/// Updated for Sentry SDK 6.0.0 - includes FeedbackId from CaptureFeedback return value.
/// </summary>
public sealed class FeedbackResult
{
    /// <summary>
    /// Gets whether the submission was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets the associated event ID if available.
    /// </summary>
    public SentryEventId? EventId { get; set; }

    /// <summary>
    /// Gets the feedback event ID returned by Sentry.
    /// New in Sentry SDK 6.0.0.
    /// </summary>
    public SentryEventId? FeedbackId { get; set; }

    /// <summary>
    /// Gets any error message if the submission failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static FeedbackResult Ok(SentryEventId? eventId = null) =>
        new() { Success = true, EventId = eventId };

    /// <summary>
    /// Creates a successful result with feedback ID.
    /// </summary>
    /// <param name="eventId">The associated event ID.</param>
    /// <param name="feedbackId">The feedback event ID returned by Sentry.</param>
    public static FeedbackResult Ok(SentryEventId eventId, SentryEventId feedbackId) =>
        new() { Success = true, EventId = eventId, FeedbackId = feedbackId };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static FeedbackResult Failed(string message) =>
        new() { Success = false, ErrorMessage = message };
}
