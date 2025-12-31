namespace MySentry.Plugin.Enrichers;

/// <summary>
/// Enriches Sentry events with additional context before they are sent.
/// Implement this interface to add custom data to all events.
/// </summary>
public interface IEventEnricher
{
    /// <summary>
    /// Gets the order in which this enricher should be executed.
    /// Lower values execute first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Enriches an event with additional context.
    /// </summary>
    /// <param name="context">The enrichment context containing event information.</param>
    void Enrich(EventEnrichmentContext context);
}
