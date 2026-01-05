using Microsoft.Extensions.Logging;

namespace MySentry.Plugin.Configuration;

/// <summary>
/// Configuration options for the MySentry plugin.
/// Supports both programmatic configuration and binding from appsettings.json.
/// </summary>
public sealed class SentryPluginOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "MySentry";

    /// <summary>
    /// Gets or sets the Sentry Data Source Name (DSN).
    /// Can also be set via the SENTRY_DSN environment variable.
    /// </summary>
    public string? Dsn { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the SDK is enabled.
    /// When false, no events will be captured or sent.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether debug mode is enabled.
    /// When enabled, SDK diagnostic messages are logged.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    /// Gets or sets the diagnostic log level when debug mode is enabled.
    /// </summary>
    public DiagnosticLevel DiagnosticLevel { get; set; } = DiagnosticLevel.Warning;

    /// <summary>
    /// Gets or sets the environment name (e.g., "production", "staging", "development").
    /// Can also be set via the SENTRY_ENVIRONMENT environment variable.
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the release version.
    /// Can also be set via the SENTRY_RELEASE environment variable.
    /// </summary>
    public string? Release { get; set; }

    /// <summary>
    /// Gets or sets the server name.
    /// Defaults to the machine name if not specified.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send default PII.
    /// When true, includes user IP address and other potentially identifying information.
    /// </summary>
    public bool SendDefaultPii { get; set; }

    /// <summary>
    /// Gets or sets the sample rate for error events (0.0 to 1.0).
    /// 1.0 means all events are sent, 0.5 means 50% of events are sent.
    /// </summary>
    public double SampleRate { get; set; } = SamplingRates.All;

    /// <summary>
    /// Gets or sets the maximum number of breadcrumbs to capture.
    /// </summary>
    public int MaxBreadcrumbs { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether to attach stack traces to pure capture message calls.
    /// </summary>
    public bool AttachStacktrace { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum breadcrumb log level to capture.
    /// </summary>
    public LogLevel MinimumBreadcrumbLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets the minimum event log level to capture.
    /// </summary>
    public LogLevel MinimumEventLevel { get; set; } = LogLevel.Error;

    /// <summary>
    /// Gets or sets the shutdown timeout.
    /// The SDK waits this long to flush pending events before shutting down.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the tracing configuration options.
    /// </summary>
    public TracingOptions Tracing { get; set; } = new();

    /// <summary>
    /// Gets or sets the profiling configuration options.
    /// </summary>
    public ProfilingOptions Profiling { get; set; } = new();

    /// <summary>
    /// Gets or sets the filtering configuration options.
    /// </summary>
    public FilteringOptions Filtering { get; set; } = new();

    /// <summary>
    /// Gets or sets the request body size to capture.
    /// </summary>
    public RequestBodySize MaxRequestBodySize { get; set; } = RequestBodySize.None;

    /// <summary>
    /// Gets or sets a value indicating whether to enable structured logs capture.
    /// </summary>
    public bool EnableLogs { get; set; }

    /// <summary>
    /// Gets the list of namespace prefixes to include as "in-app" code.
    /// </summary>
    public List<string> InAppInclude { get; set; } = new();

    /// <summary>
    /// Gets the list of namespace prefixes to exclude from "in-app" code.
    /// </summary>
    public List<string> InAppExclude { get; set; } = new();

    /// <summary>
    /// Gets the default tags to apply to all events.
    /// </summary>
    public Dictionary<string, string> DefaultTags { get; set; } = new();

    /// <summary>
    /// Gets or sets the data scrubbing options for sensitive data handling.
    /// </summary>
    public DataScrubbingOptions DataScrubbing { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to capture response headers in error events.
    /// </summary>
    public bool CaptureResponseHeaders { get; set; }

    /// <summary>
    /// Gets the list of response headers to capture.
    /// Only used when <see cref="CaptureResponseHeaders"/> is true.
    /// </summary>
    public List<string> ResponseHeadersToCapture { get; set; } = new()
    {
        "Content-Type",
        "Content-Length",
        "Cache-Control",
        "X-Request-Id",
        "X-Correlation-Id"
    };

    /// <summary>
    /// Gets or sets the callback invoked before an event is sent to Sentry.
    /// Return false to discard the event.
    /// </summary>
    /// <remarks>
    /// Use this to:
    /// - Scrub sensitive data from events
    /// - Add custom tags or context
    /// - Filter events based on custom logic
    /// </remarks>
    public SentryCallbacks.BeforeSendCallback? BeforeSend { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked before a breadcrumb is captured.
    /// Return false to discard the breadcrumb.
    /// </summary>
    /// <remarks>
    /// Use this to:
    /// - Filter noisy breadcrumbs
    /// - Scrub sensitive data from breadcrumb messages
    /// </remarks>
    public SentryCallbacks.BeforeBreadcrumbCallback? BeforeBreadcrumb { get; set; }

    /// <summary>
    /// Gets or sets the callback for dynamic transaction sampling.
    /// Return a sample rate (0.0-1.0) or null to use the default rate.
    /// </summary>
    /// <remarks>
    /// This callback is mutually exclusive with <see cref="TracingOptions.SampleRate"/>.
    /// When set, it takes precedence over the static sample rate.
    /// </remarks>
    public SentryCallbacks.TracesSamplerCallback? TracesSampler { get; set; }
}
