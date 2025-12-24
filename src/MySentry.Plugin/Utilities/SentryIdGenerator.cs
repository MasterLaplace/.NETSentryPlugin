using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Utilities;

/// <summary>
/// Generates and manages Sentry event IDs.
/// </summary>
public static class SentryIdGenerator
{
    /// <summary>
    /// Generates a new unique Sentry event ID.
    /// </summary>
    /// <returns>A new event ID.</returns>
    public static SentryEventId Generate() => new(Guid.NewGuid());

    /// <summary>
    /// Parses a string representation of an event ID.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed event ID.</returns>
    /// <exception cref="FormatException">Thrown when the value is not a valid GUID.</exception>
    public static SentryEventId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new FormatException($"'{value}' is not a valid Sentry event ID.");
        }

        return new SentryEventId(guid);
    }

    /// <summary>
    /// Tries to parse a string representation of an event ID.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="eventId">The parsed event ID if successful.</param>
    /// <returns>True if the parsing was successful, false otherwise.</returns>
    public static bool TryParse(string? value, out SentryEventId eventId)
    {
        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var guid))
        {
            eventId = SentryEventId.Empty;
            return false;
        }

        eventId = new SentryEventId(guid);
        return true;
    }
}
