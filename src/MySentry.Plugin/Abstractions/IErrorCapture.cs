namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides exception and error capture capabilities.
/// Implements the strategy pattern for customizable error handling behavior.
/// </summary>
public interface IErrorCapture
{
    /// <summary>
    /// Captures an exception and transmits it to Sentry with full context enrichment.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <returns>The Sentry event ID for tracking purposes.</returns>
    SentryEventId CaptureException(Exception exception);

    /// <summary>
    /// Captures an exception with additional scope configuration.
    /// </summary>
    /// <param name="exception">The exception to capture.</param>
    /// <param name="configureScope">Action to configure the scope for this specific capture.</param>
    /// <returns>The Sentry event ID for tracking purposes.</returns>
    SentryEventId CaptureException(Exception exception, Action<ISentryScope> configureScope);

    /// <summary>
    /// Captures a message with the specified severity level.
    /// </summary>
    /// <param name="message">The message to capture.</param>
    /// <param name="level">The severity level of the message.</param>
    /// <returns>The Sentry event ID for tracking purposes.</returns>
    SentryEventId CaptureMessage(string message, SeverityLevel level = SeverityLevel.Info);

    /// <summary>
    /// Captures a message with additional scope configuration.
    /// </summary>
    /// <param name="message">The message to capture.</param>
    /// <param name="level">The severity level of the message.</param>
    /// <param name="configureScope">Action to configure the scope for this specific capture.</param>
    /// <returns>The Sentry event ID for tracking purposes.</returns>
    SentryEventId CaptureMessage(string message, SeverityLevel level, Action<ISentryScope> configureScope);
}
