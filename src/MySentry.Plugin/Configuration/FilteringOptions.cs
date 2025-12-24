namespace MySentry.Plugin.Configuration;

/// <summary>
/// Configuration options for event filtering.
/// </summary>
public sealed class FilteringOptions
{
    /// <summary>
    /// Gets the list of exception types to ignore.
    /// These exceptions will not be captured or sent to Sentry.
    /// </summary>
    public List<string> IgnoreExceptionTypes { get; set; } = new()
    {
        "System.OperationCanceledException",
        "System.Threading.Tasks.TaskCanceledException"
    };

    /// <summary>
    /// Gets the list of URL patterns to ignore for error capture.
    /// Supports wildcards (e.g., "/health*").
    /// </summary>
    public List<string> IgnoreUrls { get; set; } = new();

    /// <summary>
    /// Gets the list of HTTP status codes to ignore.
    /// Errors returning these status codes will not be captured.
    /// </summary>
    public List<int> IgnoreStatusCodes { get; set; } = new() { 404 };

    /// <summary>
    /// Gets the list of message patterns to ignore.
    /// Events with messages matching these patterns will not be captured.
    /// </summary>
    public List<string> IgnoreMessages { get; set; } = new();

    /// <summary>
    /// Gets the list of user agent patterns to ignore.
    /// Requests from matching user agents will not generate events.
    /// </summary>
    public List<string> IgnoreUserAgents { get; set; } = new()
    {
        "health*",
        "kube-probe/*",
        "GoogleHC/*",
        "ELB-HealthChecker/*"
    };
}
