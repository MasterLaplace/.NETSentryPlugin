namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Provides breadcrumb tracking capabilities for navigation trails.
/// Breadcrumbs are a trail of events that led up to an error.
/// </summary>
public interface IBreadcrumbTracker
{
    /// <summary>
    /// Adds a breadcrumb to the current scope.
    /// </summary>
    /// <param name="message">The message describing the event.</param>
    /// <param name="category">The category of the breadcrumb (e.g., "navigation", "http", "query").</param>
    /// <param name="type">The type of breadcrumb (e.g., "default", "http", "navigation", "error").</param>
    /// <param name="level">The severity level of the breadcrumb.</param>
    void AddBreadcrumb(
        string message,
        string? category = null,
        string? type = null,
        BreadcrumbLevel level = BreadcrumbLevel.Info);

    /// <summary>
    /// Adds a breadcrumb with additional data to the current scope.
    /// </summary>
    /// <param name="message">The message describing the event.</param>
    /// <param name="category">The category of the breadcrumb.</param>
    /// <param name="type">The type of breadcrumb.</param>
    /// <param name="data">Additional data to attach to the breadcrumb.</param>
    /// <param name="level">The severity level of the breadcrumb.</param>
    void AddBreadcrumb(
        string message,
        string? category,
        string? type,
        IReadOnlyDictionary<string, string>? data,
        BreadcrumbLevel level = BreadcrumbLevel.Info);

    /// <summary>
    /// Adds an HTTP breadcrumb for tracking HTTP requests.
    /// </summary>
    /// <param name="method">The HTTP method (GET, POST, etc.).</param>
    /// <param name="url">The URL of the request.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    void AddHttpBreadcrumb(string method, string url, int? statusCode = null);

    /// <summary>
    /// Adds a navigation breadcrumb for tracking navigation events.
    /// </summary>
    /// <param name="from">The source location.</param>
    /// <param name="to">The destination location.</param>
    void AddNavigationBreadcrumb(string from, string to);

    /// <summary>
    /// Adds a query breadcrumb for tracking database queries.
    /// </summary>
    /// <param name="query">The query that was executed.</param>
    /// <param name="category">The category (e.g., "sql", "elasticsearch").</param>
    void AddQueryBreadcrumb(string query, string category = "query");
}
