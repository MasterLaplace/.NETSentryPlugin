using System.Reflection;
using System.Runtime.InteropServices;

namespace MySentry.Plugin.Utilities;

/// <summary>
/// Detects the current runtime environment.
/// </summary>
public static class EnvironmentDetector
{
    private static readonly Lazy<string> CachedEnvironment = new(DetectEnvironment);

    /// <summary>
    /// Gets the detected environment name.
    /// </summary>
    public static string DetectedEnvironment => CachedEnvironment.Value;

    /// <summary>
    /// Gets a value indicating whether the current environment is development.
    /// </summary>
    public static bool IsDevelopment =>
        DetectedEnvironment.Equals("development", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the current environment is production.
    /// </summary>
    public static bool IsProduction =>
        DetectedEnvironment.Equals("production", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the current environment is staging.
    /// </summary>
    public static bool IsStaging =>
        DetectedEnvironment.Equals("staging", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the application is running in a container.
    /// </summary>
    public static bool IsRunningInContainer =>
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
        File.Exists("/.dockerenv");

    /// <summary>
    /// Gets a value indicating whether the application is running in Kubernetes.
    /// </summary>
    public static bool IsRunningInKubernetes =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));

    /// <summary>
    /// Gets a value indicating whether the application is running in Azure.
    /// </summary>
    public static bool IsRunningInAzure =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"));

    /// <summary>
    /// Gets a value indicating whether the application is running in AWS.
    /// </summary>
    public static bool IsRunningInAws =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_EXECUTION_ENV"));

    /// <summary>
    /// Gets the current release version from the entry assembly.
    /// </summary>
    public static string? GetReleaseVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
        {
            return null;
        }

        var version = assembly.GetName().Version;
        if (version is null)
        {
            return null;
        }

        return version.ToString();
    }

    /// <summary>
    /// Gets the informational version from the entry assembly.
    /// </summary>
    public static string? GetInformationalVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        var attribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return attribute?.InformationalVersion;
    }

    private static string DetectEnvironment()
    {
        // Check common environment variables
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("SENTRY_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ENVIRONMENT");

        if (!string.IsNullOrEmpty(env))
        {
            return env.ToLowerInvariant();
        }

        // Check for Azure environment
        var azureEnv = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        if (!string.IsNullOrEmpty(azureEnv))
        {
            return azureEnv.ToLowerInvariant();
        }

        // Check if debugger is attached
        if (System.Diagnostics.Debugger.IsAttached)
        {
            return "development";
        }

        // Default to production
        return "production";
    }
}
