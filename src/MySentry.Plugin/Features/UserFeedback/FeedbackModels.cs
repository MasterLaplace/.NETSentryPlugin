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
/// </summary>
public sealed class FeedbackResult
{
    /// <summary>
    /// Gets whether the submission was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets the event ID if available.
    /// </summary>
    public SentryEventId? EventId { get; set; }

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
    /// Creates a failed result.
    /// </summary>
    public static FeedbackResult Failed(string message) =>
        new() { Success = false, ErrorMessage = message };
}
