namespace MySentry.Plugin.Abstractions;

/// <summary>
/// Represents user information for Sentry events.
/// Used to identify and correlate events to specific users.
/// </summary>
public sealed class SentryUser
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// This is typically a database ID or UUID.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the username or display name of the user.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the user.
    /// Use "{{auto}}" to let Sentry automatically capture the IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the segment the user belongs to (e.g., "premium", "enterprise").
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    /// Gets the additional data about the user.
    /// </summary>
    public Dictionary<string, string> AdditionalData { get; } = new();

    /// <summary>
    /// Creates an empty user.
    /// </summary>
    public SentryUser()
    {
    }

    /// <summary>
    /// Creates a user with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    public SentryUser(string id)
    {
        Id = id;
    }

    /// <summary>
    /// Creates a user with the specified ID and email.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="email">The email address of the user.</param>
    public SentryUser(string id, string email)
    {
        Id = id;
        Email = email;
    }

    /// <summary>
    /// Creates a user with the specified ID, email, and username.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <param name="username">The username or display name of the user.</param>
    public SentryUser(string id, string email, string username)
    {
        Id = id;
        Email = email;
        Username = username;
    }

    /// <summary>
    /// Sets additional data on the user.
    /// </summary>
    /// <param name="key">The data key.</param>
    /// <param name="value">The data value.</param>
    /// <returns>This user instance for fluent chaining.</returns>
    public SentryUser WithData(string key, string value)
    {
        AdditionalData[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the IP address and returns this instance.
    /// </summary>
    /// <param name="ipAddress">The IP address, or "{{auto}}" for automatic capture.</param>
    /// <returns>This user instance for fluent chaining.</returns>
    public SentryUser WithIpAddress(string ipAddress)
    {
        IpAddress = ipAddress;
        return this;
    }

    /// <summary>
    /// Sets the IP address to automatic capture and returns this instance.
    /// </summary>
    /// <returns>This user instance for fluent chaining.</returns>
    public SentryUser WithAutoIpAddress()
    {
        IpAddress = "{{auto}}";
        return this;
    }

    /// <summary>
    /// Sets the segment and returns this instance.
    /// </summary>
    /// <param name="segment">The segment the user belongs to.</param>
    /// <returns>This user instance for fluent chaining.</returns>
    public SentryUser WithSegment(string segment)
    {
        Segment = segment;
        return this;
    }
}
