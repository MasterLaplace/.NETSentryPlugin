namespace MySentry.Plugin.Configuration;

/// <summary>
/// Configuration options for distributed tracing.
/// </summary>
public sealed class TracingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether tracing is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the sample rate for traces (0.0 to 1.0).
    /// </summary>
    public double SampleRate { get; set; } = SamplingRates.Disabled;

    /// <summary>
    /// Gets or sets a value indicating whether to trace all incoming HTTP requests.
    /// </summary>
    public bool TraceAllRequests { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trace database operations.
    /// </summary>
    public bool TraceDatabase { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trace outgoing HTTP requests.
    /// </summary>
    public bool TraceHttpClients { get; set; } = true;

    /// <summary>
    /// Gets the list of URL patterns to ignore when tracing.
    /// Supports wildcards (e.g., "/health*", "/api/internal/*").
    /// </summary>
    public List<string> IgnoreUrls { get; set; } = new()
    {
        "/health",
        "/health/*",
        "/healthz",
        "/metrics",
        "/ready",
        "/readyz",
        "/live",
        "/livez",
        "/favicon.ico"
    };

    /// <summary>
    /// Gets the list of transaction names to ignore.
    /// </summary>
    public List<string> IgnoreTransactions { get; set; } = new();
}
