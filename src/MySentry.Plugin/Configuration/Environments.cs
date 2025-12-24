namespace MySentry.Plugin.Configuration;

/// <summary>
/// Defines well-known environment names for Sentry configuration.
/// Use these constants instead of magic strings.
/// </summary>
public static class Environments
{
    /// <summary>
    /// Local development environment.
    /// </summary>
    public const string Development = "development";

    /// <summary>
    /// Staging or pre-production environment.
    /// </summary>
    public const string Staging = "staging";

    /// <summary>
    /// Production environment.
    /// </summary>
    public const string Production = "production";

    /// <summary>
    /// Testing environment for automated tests.
    /// </summary>
    public const string Testing = "testing";

    /// <summary>
    /// Quality Assurance environment.
    /// </summary>
    public const string QA = "qa";

    /// <summary>
    /// User Acceptance Testing environment.
    /// </summary>
    public const string UAT = "uat";
}
