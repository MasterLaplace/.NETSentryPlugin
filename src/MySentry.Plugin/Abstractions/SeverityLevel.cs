namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Defines the severity level of a Sentry event.
/// Used to categorize events by importance and impact.
/// </summary>
public enum SeverityLevel
{
    /// <summary>
    /// Debug-level messages, typically used for development diagnostics.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Informational messages that highlight the progress of the application.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning messages that indicate potential issues or deprecated functionality.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error messages that indicate failures that should be investigated.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Critical messages that indicate severe failures requiring immediate attention.
    /// </summary>
    Fatal = 4
}
