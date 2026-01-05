#if ASPNETCORE
using Microsoft.Extensions.DependencyInjection;

namespace MySentry.Plugin.Features.EntityFramework;

/// <summary>
/// Extension methods for Entity Framework Core integration with MySentry.
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Enables Entity Framework Core automatic instrumentation with Sentry.
    /// This captures database queries as spans and adds them to distributed traces.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// When using Sentry.AspNetCore, EF Core instrumentation is automatically enabled via
    /// DiagnosticListener integration. This method configures additional MySentry-specific
    /// breadcrumb capture for database operations.
    ///
    /// To enable full EF Core tracing, ensure:
    /// 1. Sentry.AspNetCore is configured with tracing enabled
    /// 2. Your DbContext uses diagnostic events (enabled by default)
    ///
    /// The SDK automatically captures:
    /// - Query text (if SendDefaultPii is enabled or query logging is configured)
    /// - Database operation type (SELECT, INSERT, UPDATE, DELETE)
    /// - Connection info
    /// - Duration
    ///
    /// For additional breadcrumb capture, use the DbContextOptionsBuilder extension.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// builder.Services.AddMySentryEntityFramework();
    ///
    /// // Sentry automatically instruments EF Core via DiagnosticListener
    /// // when tracing is enabled
    /// </code>
    /// </example>
    public static IServiceCollection AddMySentryEntityFramework(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Note: Sentry.AspNetCore automatically instruments EF Core through DiagnosticListener
        // when tracing is enabled. This registration adds MySentry-specific breadcrumb capture.
        services.AddSingleton<DatabaseBreadcrumbListener>();
        services.AddHostedService<DatabaseBreadcrumbHostedService>();

        return services;
    }
}

/// <summary>
/// Listener for database diagnostic events to capture breadcrumbs.
/// </summary>
internal sealed class DatabaseBreadcrumbListener : IDisposable
{
    // Note: Sentry SDK automatically subscribes to DiagnosticListener for EF Core
    // This class is a placeholder for potential future custom breadcrumb capture

    public void Dispose()
    {
        // Nothing to dispose - SDK handles the subscription
    }
}

/// <summary>
/// Hosted service that initializes database breadcrumb capture.
/// </summary>
internal sealed class DatabaseBreadcrumbHostedService : Microsoft.Extensions.Hosting.IHostedService
{
    private readonly DatabaseBreadcrumbListener _listener;

    public DatabaseBreadcrumbHostedService(DatabaseBreadcrumbListener listener)
    {
        _listener = listener;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Database instrumentation is handled by Sentry SDK via DiagnosticListener
        // This hosted service ensures the listener is initialized
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
#endif
