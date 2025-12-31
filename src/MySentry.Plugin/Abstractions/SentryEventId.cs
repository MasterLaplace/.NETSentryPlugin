namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Represents a unique identifier for a Sentry event.
/// Wraps the underlying Sentry SDK's event ID for type safety and abstraction.
/// </summary>
public readonly struct SentryEventId : IEquatable<SentryEventId>
{
    /// <summary>
    /// Gets the underlying GUID value of the event ID.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new SentryEventId with the specified value.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    public SentryEventId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new SentryEventId from a Sentry SDK event ID.
    /// </summary>
    /// <param name="sentryId">The Sentry SDK event ID.</param>
    public SentryEventId(Sentry.SentryId sentryId)
    {
        Value = sentryId.Equals(Sentry.SentryId.Empty) ? Guid.Empty : Guid.Parse(sentryId.ToString());
    }

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

    /// <inheritdoc/>
    public bool Equals(SentryEventId other) => Value.Equals(other.Value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SentryEventId other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

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
    /// Equality operator.
    /// </summary>
    public static bool operator ==(SentryEventId left, SentryEventId right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(SentryEventId left, SentryEventId right) => !left.Equals(right);

    /// <summary>
    /// Creates a new random event ID.
    /// </summary>
    /// <returns>A new unique event ID.</returns>
    public static SentryEventId Create() => new(Guid.NewGuid());
}
