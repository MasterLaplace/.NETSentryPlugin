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
    /// Critical breadcrumb, indicating a severe failure.
    /// </summary>
    Critical = 4
}
