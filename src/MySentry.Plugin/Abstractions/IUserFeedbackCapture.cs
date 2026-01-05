namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides user feedback capture capabilities.
/// User feedback allows users to provide additional context about errors.
/// Updated for Sentry SDK 6.0.0 - CaptureFeedback now returns a SentryId.
/// </summary>
public interface IUserFeedbackCapture
{
    /// <summary>
    /// Captures user feedback associated with an event.
    /// </summary>
    /// <param name="eventId">The event ID to associate the feedback with.</param>
    /// <param name="name">The user's name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="comments">The user's feedback comments.</param>
    /// <returns>The feedback event ID for tracking purposes.</returns>
    SentryEventId CaptureFeedback(SentryEventId eventId, string name, string email, string comments);

    /// <summary>
    /// Captures user feedback with detailed information.
    /// </summary>
    /// <param name="feedback">The feedback to capture.</param>
    /// <returns>The feedback event ID for tracking purposes.</returns>
    SentryEventId CaptureFeedback(UserFeedback feedback);
}

/// <summary>
/// Represents user feedback for a Sentry event.
/// </summary>
public sealed class UserFeedback
{
    /// <summary>
    /// Gets or sets the event ID this feedback is associated with.
    /// </summary>
    public SentryEventId EventId { get; set; }

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the user's feedback comments.
    /// </summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new user feedback instance.
    /// </summary>
    public UserFeedback()
    {
    }

    /// <summary>
    /// Creates a new user feedback instance with the specified details.
    /// </summary>
    /// <param name="eventId">The event ID to associate the feedback with.</param>
    /// <param name="comments">The user's feedback comments.</param>
    public UserFeedback(SentryEventId eventId, string comments)
    {
        EventId = eventId;
        Comments = comments;
    }

    /// <summary>
    /// Creates a new user feedback instance with full details.
    /// </summary>
    /// <param name="eventId">The event ID to associate the feedback with.</param>
    /// <param name="name">The user's name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="comments">The user's feedback comments.</param>
    public UserFeedback(SentryEventId eventId, string name, string email, string comments)
    {
        EventId = eventId;
        Name = name;
        Email = email;
        Comments = comments;
    }
}
