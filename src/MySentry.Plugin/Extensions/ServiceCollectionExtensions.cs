#if ASPNETCORE
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Configuration;
using MySentry.Plugin.Core;
using MySentry.Plugin.Enrichers;
using Sentry.AspNetCore;

namespace MySentry.Plugin.Extensions;

/// <summary>
/// Extension methods for adding MySentry services to the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MySentry plugin services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind options from.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySentry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddMySentry(configuration, _ => { });
    }

    /// <summary>
    /// Adds MySentry plugin services to the service collection with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind options from.</param>
    /// <param name="configure">Action to configure the plugin.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySentry(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SentryPluginBuilder> configure)
    {
        var builder = new SentryPluginBuilder();
        configure(builder);
        builder.ApplyConfiguration();

        // Bind from configuration section
        services.Configure<SentryPluginOptions>(
            configuration.GetSection(SentryPluginOptions.SectionName));

        // Apply builder options on top of configuration
        services.PostConfigure<SentryPluginOptions>(options =>
        {
            ApplyBuilderOptions(options, builder.Options);
        });

        RegisterServices(services, builder);

        return services;
    }

    /// <summary>
    /// Adds MySentry plugin services to the service collection with programmatic configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the plugin.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMySentry(
        this IServiceCollection services,
        Action<SentryPluginBuilder> configure)
    {
        var builder = new SentryPluginBuilder();
        configure(builder);
        builder.ApplyConfiguration();

        services.Configure<SentryPluginOptions>(options =>
        {
            ApplyBuilderOptions(options, builder.Options);
        });

        RegisterServices(services, builder);

        return services;
    }

    private static void RegisterServices(IServiceCollection services, SentryPluginBuilder builder)
    {
        services.AddHttpContextAccessor();

        // Register Sentry SDK, IHub, and plugin services only if DSN is provided
        var dsn = builder.Options.Dsn;
        if (!string.IsNullOrWhiteSpace(dsn))
        {
            services.AddSentry();
            services.AddSingleton<IHub>(sp => sp.GetRequiredService<Sentry.IHub>());
            services.AddSingleton<ISentryPlugin, SentryPlugin>();
            services.AddSingleton<IErrorCapture>(sp => sp.GetRequiredService<ISentryPlugin>());
            services.AddSingleton<IPerformanceMonitor>(sp => sp.GetRequiredService<ISentryPlugin>());
            services.AddSingleton<IBreadcrumbTracker>(sp => sp.GetRequiredService<ISentryPlugin>());
            services.AddSingleton<IUserContextProvider>(sp => sp.GetRequiredService<ISentryPlugin>());
            services.AddSingleton<IScopeManager>(sp => sp.GetRequiredService<ISentryPlugin>());
            services.AddSingleton<IUserFeedbackCapture>(sp =>
                (IUserFeedbackCapture)sp.GetRequiredService<ISentryPlugin>());
            services.AddSingleton<ICronMonitor>(sp =>
                (ICronMonitor)sp.GetRequiredService<ISentryPlugin>());
        }

        // Register enrichers
        foreach (var enricherType in builder.EnricherTypes)
        {
            services.AddTransient(typeof(IEventEnricher), enricherType);
        }

        foreach (var enricherInstance in builder.EnricherInstances)
        {
            services.AddSingleton(typeof(IEventEnricher), enricherInstance);
        }

        // Register default enrichers
        services.AddTransient<IEventEnricher, EnvironmentEnricher>();
        services.AddTransient<IEventEnricher, RequestEnricher>();
        services.AddTransient<IEventEnricher, UserEnricher>();
    }

    private static void ApplyBuilderOptions(SentryPluginOptions target, SentryPluginOptions source)
    {
        if (!string.IsNullOrEmpty(source.Dsn))
        {
            target.Dsn = source.Dsn;
        }

        if (!string.IsNullOrEmpty(source.Environment))
        {
            target.Environment = source.Environment;
        }

        if (!string.IsNullOrEmpty(source.Release))
        {
            target.Release = source.Release;
        }

        if (!string.IsNullOrEmpty(source.ServerName))
        {
            target.ServerName = source.ServerName;
        }

        if (source.Debug)
        {
            target.Debug = source.Debug;
            target.DiagnosticLevel = source.DiagnosticLevel;
        }

        if (source.SendDefaultPii)
        {
            target.SendDefaultPii = source.SendDefaultPii;
        }

        if (source.SampleRate < 1.0)
        {
            target.SampleRate = source.SampleRate;
        }

        if (source.MaxBreadcrumbs != 100)
        {
            target.MaxBreadcrumbs = source.MaxBreadcrumbs;
        }

        if (source.ShutdownTimeout != TimeSpan.FromSeconds(2))
        {
            target.ShutdownTimeout = source.ShutdownTimeout;
        }

        if (source.Tracing.Enabled)
        {
            target.Tracing = source.Tracing;
        }

        if (source.Profiling.Enabled)
        {
            target.Profiling = source.Profiling;
        }

        if (source.MaxRequestBodySize != RequestBodySize.None)
        {
            target.MaxRequestBodySize = source.MaxRequestBodySize;
        }

        if (source.EnableLogs)
        {
            target.EnableLogs = source.EnableLogs;
        }

        target.InAppInclude.AddRange(source.InAppInclude);
        target.InAppExclude.AddRange(source.InAppExclude);
        target.Filtering.IgnoreExceptionTypes.AddRange(
            source.Filtering.IgnoreExceptionTypes.Except(target.Filtering.IgnoreExceptionTypes));
        target.Filtering.IgnoreUrls.AddRange(source.Filtering.IgnoreUrls);
    }
}
#endif
