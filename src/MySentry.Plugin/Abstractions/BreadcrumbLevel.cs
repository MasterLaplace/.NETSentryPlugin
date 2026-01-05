namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Defines the severity level of a breadcrumb.
/// Used to categorize breadcrumbs by importance.
/// </summary>
public enum BreadcrumbLevel
{
    /// <summary>
    /// Debug-level breadcrumb, used for development diagnostics.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Informational breadcrumb, indicating normal operation.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning-level breadcrumb, indicating potential issues.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error-level breadcrumb, indicating a failure.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Fatal breadcrumb, indicating a severe failure.
    /// Renamed from Critical in Sentry SDK 6.0.0 for consistency with other Sentry SDKs.
    /// </summary>
    Fatal = 4,

    /// <summary>
    /// Alias for Fatal to maintain backward compatibility.
    /// </summary>
    [Obsolete("Use BreadcrumbLevel.Fatal instead. Critical was renamed to Fatal in Sentry SDK 6.0.0.")]
    Critical = Fatal
}
