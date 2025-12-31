using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Features.Releases;

/// <summary>
/// Provides release tracking functionality.
/// </summary>
public sealed class ReleaseTracker
{
    private readonly ISentryPlugin _plugin;

    /// <summary>
    /// Creates a new release tracker.
    /// </summary>
    /// <param name="plugin">The Sentry plugin.</param>
    public ReleaseTracker(ISentryPlugin plugin)
    {
        _plugin = plugin;
    }

    /// <summary>
    /// Sets the release version for all subsequent events.
    /// </summary>
    /// <param name="version">The release version.</param>
    public void SetRelease(string version)
    {
        _plugin.ConfigureScope(scope =>
        {
            scope.SetTag("release", version);
        });
    }

    /// <summary>
    /// Sets the release with detailed information.
    /// </summary>
    /// <param name="version">The version number.</param>
    /// <param name="environment">The target environment.</param>
    /// <param name="commitSha">The Git commit SHA.</param>
    public void SetRelease(string version, string? environment = null, string? commitSha = null)
    {
        _plugin.ConfigureScope(scope =>
        {
            scope.SetTag("release", version);

            if (!string.IsNullOrEmpty(environment))
            {
                scope.SetTag("environment", environment!);
            }

            if (!string.IsNullOrEmpty(commitSha))
            {
                scope.SetTag("commit", commitSha!);
            }
        });
    }

    /// <summary>
    /// Adds deployment information to the current context.
    /// </summary>
    /// <param name="deploymentId">The deployment ID.</param>
    /// <param name="deployedBy">Who performed the deployment.</param>
    /// <param name="deployedAt">When the deployment occurred.</param>
    public void SetDeployment(string deploymentId, string? deployedBy = null, DateTimeOffset? deployedAt = null)
    {
        _plugin.ConfigureScope(scope =>
        {
            scope.SetContext("Deployment", new
            {
                Id = deploymentId,
                DeployedBy = deployedBy,
                DeployedAt = deployedAt ?? DateTimeOffset.UtcNow
            });
        });
    }
}
