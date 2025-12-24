namespace MySentry.Plugin.Configuration;

/// <summary>
/// Defines well-known sampling rate constants for Sentry configuration.
/// Use these constants instead of magic numbers.
/// </summary>
public static class SamplingRates
{
    /// <summary>
    /// Disable sampling - no events will be captured.
    /// </summary>
    public const double Disabled = 0.0;

    /// <summary>
    /// Minimal sampling - captures only 1% of events.
    /// Suitable for very high-traffic applications.
    /// </summary>
    public const double Minimal = 0.01;

    /// <summary>
    /// Low sampling - captures 10% of events.
    /// Suitable for high-traffic production environments.
    /// </summary>
    public const double Low = 0.1;

    /// <summary>
    /// Standard sampling - captures 25% of events.
    /// Good balance for most production environments.
    /// </summary>
    public const double Standard = 0.25;

    /// <summary>
    /// Recommended production sampling - captures 50% of events.
    /// Provides good visibility while managing costs.
    /// </summary>
    public const double RecommendedProduction = 0.5;

    /// <summary>
    /// High sampling - captures 75% of events.
    /// Suitable for staging or when more visibility is needed.
    /// </summary>
    public const double High = 0.75;

    /// <summary>
    /// Full sampling - captures all events.
    /// Recommended for development and testing only.
    /// </summary>
    public const double All = 1.0;

    /// <summary>
    /// Development sampling - captures all events.
    /// Same as <see cref="All"/>, but named for clarity.
    /// </summary>
    public const double Development = 1.0;
}
