namespace MySentry.Plugin.Configuration;

/// <summary>
/// Defines the diagnostic log level for the Sentry SDK.
/// </summary>
public enum DiagnosticLevel
{
    /// <summary>
    /// Debug level - most verbose, includes all diagnostic messages.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Info level - general operational messages.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning level - potential issues that don't prevent operation.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error level - errors that affect SDK operation.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Fatal level - critical errors that prevent SDK operation.
    /// </summary>
    Fatal = 4
}

/// <summary>
/// Defines the maximum request body size to capture.
/// </summary>
public enum RequestBodySize
{
    /// <summary>
    /// Don't capture request body.
    /// </summary>
    None = 0,

    /// <summary>
    /// Capture small request bodies (up to 1KB).
    /// </summary>
    Small = 1,

    /// <summary>
    /// Capture medium request bodies (up to 10KB).
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Always capture request body regardless of size.
    /// </summary>
    Always = 3
}
