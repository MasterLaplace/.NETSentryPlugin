namespace MySentry.Plugin.Configuration;

using MySentry.Plugin.Abstractions;

/// <summary>
/// Provides callback types for customizing Sentry event processing.
/// </summary>
public static class SentryCallbacks
{
    /// <summary>
    /// Delegate for modifying or filtering events before they are sent to Sentry.
    /// </summary>
    /// <param name="eventInfo">Information about the event being processed.</param>
    /// <returns>True to send the event, false to discard it.</returns>
    /// <remarks>
    /// Use this callback to:
    /// - Scrub sensitive data from events
    /// - Add custom tags or extra data
    /// - Filter out certain events based on custom logic
    /// </remarks>
    public delegate bool BeforeSendCallback(EventProcessingInfo eventInfo);

    /// <summary>
    /// Delegate for modifying or filtering breadcrumbs before they are captured.
    /// </summary>
    /// <param name="breadcrumbInfo">Information about the breadcrumb being processed.</param>
    /// <returns>True to capture the breadcrumb, false to discard it.</returns>
    /// <remarks>
    /// Use this callback to:
    /// - Filter out noisy breadcrumbs
    /// - Scrub sensitive data from breadcrumb messages
    /// - Modify breadcrumb data before capture
    /// </remarks>
    public delegate bool BeforeBreadcrumbCallback(BreadcrumbProcessingInfo breadcrumbInfo);

    /// <summary>
    /// Delegate for dynamic transaction sampling decisions.
    /// </summary>
    /// <param name="context">Context information about the transaction being sampled.</param>
    /// <returns>Sample rate (0.0 to 1.0) or null to use the default rate.</returns>
    /// <remarks>
    /// Use this callback to:
    /// - Sample different transactions at different rates
    /// - Filter out certain transactions entirely (return 0.0)
    /// - Inherit parent sampling decisions in distributed tracing
    /// </remarks>
    public delegate double? TracesSamplerCallback(PluginTransactionSamplingContext context);
}

/// <summary>
/// Information about an event being processed by BeforeSend.
/// </summary>
public sealed class EventProcessingInfo
{
    /// <summary>
    /// Gets or sets the exception associated with the event, if any.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the event message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the event level.
    /// </summary>
    public PluginSeverityLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the transaction name, if applicable.
    /// </summary>
    public string? TransactionName { get; set; }

    /// <summary>
    /// Gets or sets the environment.
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets the modifiable tags dictionary.
    /// </summary>
    public Dictionary<string, string> Tags { get; } = new();

    /// <summary>
    /// Gets the modifiable extra data dictionary.
    /// </summary>
    public Dictionary<string, object?> Extra { get; } = new();

    /// <summary>
    /// Gets or sets custom data that can be attached to the event.
    /// </summary>
    public object? CustomData { get; set; }
}

/// <summary>
/// Information about a breadcrumb being processed by BeforeBreadcrumb.
/// </summary>
public sealed class BreadcrumbProcessingInfo
{
    /// <summary>
    /// Gets or sets the breadcrumb message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb level.
    /// </summary>
    public PluginBreadcrumbLevel Level { get; set; }

    /// <summary>
    /// Gets the modifiable data dictionary.
    /// </summary>
    public Dictionary<string, string> Data { get; } = new();

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Context information for transaction sampling decisions.
/// </summary>
public sealed class PluginTransactionSamplingContext
{
    /// <summary>
    /// Gets or sets the transaction name.
    /// </summary>
    public required string TransactionName { get; init; }

    /// <summary>
    /// Gets or sets the transaction operation type.
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    /// Gets or sets the parent sampling decision, if available.
    /// </summary>
    /// <remarks>
    /// When a transaction has a parent (from an upstream service),
    /// this contains the parent's sampling decision.
    /// In most cases, you should inherit this decision to avoid breaking distributed traces.
    /// </remarks>
    public bool? ParentSampled { get; init; }

    /// <summary>
    /// Gets or sets whether this is the root transaction.
    /// </summary>
    public bool IsRoot => ParentSampled is null;

    /// <summary>
    /// Gets custom context data.
    /// </summary>
    public IReadOnlyDictionary<string, object?> CustomData { get; init; } = new Dictionary<string, object?>();
}
