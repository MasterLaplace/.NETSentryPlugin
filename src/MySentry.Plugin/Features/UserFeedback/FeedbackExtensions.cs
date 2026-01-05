using Microsoft.Extensions.DependencyInjection;
using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.UserFeedback;

/// <summary>
/// Extension methods for user feedback functionality.
/// </summary>
public static class FeedbackExtensions
{
    /// <summary>
    /// Adds user feedback services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySentryFeedback(this IServiceCollection services)
    {
        services.AddScoped<FeedbackHandler>(provider =>
        {
            var feedbackCapture = provider.GetRequiredService<IUserFeedbackCapture>();
            return new FeedbackHandler(feedbackCapture);
        });

        return services;
    }

    /// <summary>
    /// Captures user feedback for the given event.
    /// </summary>
    /// <param name="feedbackCapture">The feedback capture service.</param>
    /// <param name="eventId">The event ID to associate feedback with.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="comments">The user's comments.</param>
    /// <param name="name">The user's name.</param>
    /// <returns>The feedback event ID for tracking purposes.</returns>
    public static PluginSentryEventId CaptureUserFeedbackFor(
        this IUserFeedbackCapture feedbackCapture,
        PluginSentryEventId eventId,
        string? email,
        string comments,
        string? name = null)
    {
        return feedbackCapture.CaptureFeedback(eventId, name ?? string.Empty, email ?? string.Empty, comments);
    }

    /// <summary>
    /// Captures an exception and attaches user feedback.
    /// </summary>
    /// <param name="feedbackCapture">The feedback capture service.</param>
    /// <param name="errorCapture">The error capture service.</param>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="comments">The user's comments.</param>
    /// <param name="name">The user's name.</param>
    /// <returns>The event ID.</returns>
    public static PluginSentryEventId CaptureExceptionWithFeedback(
        this IUserFeedbackCapture feedbackCapture,
        IErrorCapture errorCapture,
        Exception exception,
        string? email,
        string comments,
        string? name = null)
    {
        var eventId = errorCapture.CaptureException(exception);
        feedbackCapture.CaptureFeedback(eventId, name ?? string.Empty, email ?? string.Empty, comments);
        return eventId;
    }
}
