namespace MySentry.Plugin.Configuration;

/// <summary>
/// Configuration options for sensitive data scrubbing.
/// </summary>
public sealed class DataScrubbingOptions
{
    /// <summary>
    /// Gets or sets whether data scrubbing is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the replacement text for scrubbed values.
    /// Default is "[Filtered]".
    /// </summary>
    public string ReplacementText { get; set; } = "[Filtered]";

    /// <summary>
    /// Gets the list of field names to scrub (case-insensitive).
    /// Values in these fields will be replaced with <see cref="ReplacementText"/>.
    /// </summary>
    public List<string> SensitiveFields { get; set; } = new()
    {
        "password",
        "passwd",
        "secret",
        "apikey",
        "api_key",
        "apiSecret",
        "api_secret",
        "token",
        "access_token",
        "accessToken",
        "refresh_token",
        "refreshToken",
        "authorization",
        "auth",
        "credentials",
        "credit_card",
        "creditCard",
        "card_number",
        "cardNumber",
        "cvv",
        "cvc",
        "ssn",
        "social_security",
        "socialSecurity"
    };

    /// <summary>
    /// Gets the list of regex patterns to scrub from string values.
    /// Any match will be replaced with <see cref="ReplacementText"/>.
    /// </summary>
    public List<string> SensitivePatterns { get; set; } = new()
    {
        // Credit card patterns
        @"\b(?:\d{4}[-\s]?){3}\d{4}\b",
        // Email patterns (optional - uncomment if needed)
        // @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
        // SSN pattern
        @"\b\d{3}-\d{2}-\d{4}\b",
        // JWT token pattern
        @"eyJ[a-zA-Z0-9_-]*\.eyJ[a-zA-Z0-9_-]*\.[a-zA-Z0-9_-]*"
    };

    /// <summary>
    /// Gets the list of HTTP header names to scrub from request/response context.
    /// </summary>
    public List<string> SensitiveHeaders { get; set; } = new()
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-API-Key",
        "X-Auth-Token"
    };

    /// <summary>
    /// Gets or sets whether to scrub request bodies.
    /// Default is true.
    /// </summary>
    public bool ScrubRequestBodies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to scrub query strings.
    /// Default is true.
    /// </summary>
    public bool ScrubQueryStrings { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to scrub cookies.
    /// Default is true.
    /// </summary>
    public bool ScrubCookies { get; set; } = true;
}
