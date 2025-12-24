namespace MySentry.Plugin.Enrichers;

/// <summary>
/// Context provided to event enrichers containing event information.
/// </summary>
public sealed class EventEnrichmentContext
{
    private readonly Dictionary<string, string> _tags = new();
    private readonly Dictionary<string, object?> _extras = new();
    private readonly Dictionary<string, object> _contexts = new();

    /// <summary>
    /// Gets the exception associated with this event, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the message associated with this event, if any.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the severity level of this event.
    /// </summary>
    public PluginSeverityLevel Level { get; }

    /// <summary>
    /// Gets or sets the user associated with this event.
    /// </summary>
    public PluginSentryUser? User { get; set; }

    /// <summary>
    /// Gets the tags that will be attached to this event.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags => _tags;

    /// <summary>
    /// Gets the extra data that will be attached to this event.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Extras => _extras;

    /// <summary>
    /// Gets the contexts that will be attached to this event.
    /// </summary>
    public IReadOnlyDictionary<string, object> Contexts => _contexts;

    /// <summary>
    /// Creates a new enrichment context for an exception event.
    /// </summary>
    /// <param name="exception">The exception being captured.</param>
    /// <param name="level">The severity level.</param>
    public EventEnrichmentContext(Exception exception, PluginSeverityLevel level = PluginSeverityLevel.Error)
    {
        Exception = exception;
        Level = level;
    }

    /// <summary>
    /// Creates a new enrichment context for a message event.
    /// </summary>
    /// <param name="message">The message being captured.</param>
    /// <param name="level">The severity level.</param>
    public EventEnrichmentContext(string message, PluginSeverityLevel level = PluginSeverityLevel.Info)
    {
        Message = message;
        Level = level;
    }

    /// <summary>
    /// Adds a tag to the event.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    public void SetTag(string key, string value)
    {
        _tags[key] = value;
    }

    /// <summary>
    /// Adds extra data to the event.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    public void SetExtra(string key, object? value)
    {
        _extras[key] = value;
    }

    /// <summary>
    /// Adds a context to the event.
    /// </summary>
    /// <param name="key">The context key.</param>
    /// <param name="value">The context value.</param>
    public void SetContext(string key, object value)
    {
        _contexts[key] = value;
    }
}
