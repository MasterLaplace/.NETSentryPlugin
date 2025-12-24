using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.UserFeedback;

/// <summary>
/// Request model for user feedback submissions.
/// </summary>
public sealed record FeedbackRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the feedback comments.
    /// </summary>
    public required string Comments { get; init; }

    /// <summary>
    /// Gets or sets the associated event ID.
    /// </summary>
    public SentryEventId? EventId { get; init; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

/// <summary>
/// Result of a feedback submission.
/// </summary>
public sealed record FeedbackResult
{
    /// <summary>
    /// Gets whether the submission was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the event ID if available.
    /// </summary>
    public SentryEventId? EventId { get; init; }

    /// <summary>
    /// Gets any error message if the submission failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

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
