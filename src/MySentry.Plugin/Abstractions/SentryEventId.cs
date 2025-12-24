namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Represents a unique identifier for a Sentry event.
/// Wraps the underlying Sentry SDK's event ID for type safety and abstraction.
/// </summary>
/// <param name="Value">The underlying GUID value of the event ID.</param>
public readonly record struct SentryEventId(Guid Value)
{
    /// <summary>
    /// Gets an empty event ID, representing no event.
    /// </summary>
    public static SentryEventId Empty => new(Guid.Empty);

    /// <summary>
    /// Gets a value indicating whether this event ID is empty (no event).
    /// </summary>
    public bool IsEmpty => Value == Guid.Empty;

    /// <summary>
    /// Returns the string representation of the event ID.
    /// </summary>
    /// <returns>The hexadecimal string representation of the event ID.</returns>
    public override string ToString() => Value.ToString("N");

    /// <summary>
    /// Implicitly converts a <see cref="SentryEventId"/> to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="eventId">The event ID to convert.</param>
    public static implicit operator Guid(SentryEventId eventId) => eventId.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="SentryEventId"/>.
    /// </summary>
    /// <param name="guid">The GUID to convert.</param>
    public static implicit operator SentryEventId(Guid guid) => new(guid);

    /// <summary>
    /// Creates a new random event ID.
    /// </summary>
    /// <returns>A new unique event ID.</returns>
    public static SentryEventId Create() => new(Guid.NewGuid());
}
