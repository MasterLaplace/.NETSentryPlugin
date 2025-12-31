#if ASPNETCORE
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySentry.Plugin.Configuration;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;

namespace MySentry.Plugin.Extensions;

/// <summary>
/// Extension methods for configuring MySentry on <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds MySentry plugin with default configuration from appsettings.json.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder AddMySentry(this WebApplicationBuilder builder)
    {
        return builder.AddMySentry(_ => { });
    }

    /// <summary>
    /// Adds MySentry plugin with fluent configuration.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="configure">Action to configure the plugin.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder AddMySentry(
        this WebApplicationBuilder builder,
        Action<SentryPluginBuilder> configure)
    {
        var pluginBuilder = new SentryPluginBuilder();
        configure(pluginBuilder);
        pluginBuilder.ApplyConfiguration();

        // Register MySentry services
        builder.Services.AddMySentry(builder.Configuration, configure);

        // Get the plugin options
        var pluginOptions = pluginBuilder.Options;

        // Initialize Sentry SDK manually
        var dsn = GetDsn(builder.Configuration, pluginOptions);
        if (!string.IsNullOrEmpty(dsn))
        {
            SentrySdk.Init(options =>
            {
                ConfigureSentryOptions(options, builder.Configuration, pluginOptions, builder.Environment);
            });
        }

        return builder;
    }

    private static string? GetDsn(IConfiguration configuration, SentryPluginOptions pluginOptions)
    {
        if (!string.IsNullOrEmpty(pluginOptions.Dsn))
        {
            return pluginOptions.Dsn;
        }

        var section = configuration.GetSection(SentryPluginOptions.SectionName);
        return section.GetValue<string>("Dsn");
    }

    private static void ConfigureSentryOptions(
        SentryOptions options,
        IConfiguration configuration,
        SentryPluginOptions pluginOptions,
        IHostEnvironment environment)
    {
        // Bind from configuration first
        var section = configuration.GetSection(SentryPluginOptions.SectionName);
        if (section.Exists())
        {
            options.Dsn = section.GetValue<string>("Dsn") ?? options.Dsn;
            options.Debug = section.GetValue<bool>("Debug");
            options.Environment = section.GetValue<string>("Environment") ?? options.Environment;
            options.Release = section.GetValue<string>("Release") ?? options.Release;
            options.SendDefaultPii = section.GetValue<bool>("SendDefaultPii");
            options.SampleRate = (float?)section.GetValue<double?>("SampleRate") ?? options.SampleRate;
            options.MaxBreadcrumbs = section.GetValue<int?>("MaxBreadcrumbs") ?? options.MaxBreadcrumbs;
            options.AttachStacktrace = section.GetValue<bool?>("AttachStacktrace") ?? options.AttachStacktrace;
        }

        // Apply programmatic options
        if (!string.IsNullOrEmpty(pluginOptions.Dsn))
        {
            options.Dsn = pluginOptions.Dsn;
        }

        if (pluginOptions.Debug)
        {
            options.Debug = true;
            options.DiagnosticLevel = MapDiagnosticLevel(pluginOptions.DiagnosticLevel);
        }

        if (!string.IsNullOrEmpty(pluginOptions.Environment))
        {
            options.Environment = pluginOptions.Environment;
        }
        else if (string.IsNullOrEmpty(options.Environment))
        {
            options.Environment = environment.EnvironmentName.ToLowerInvariant();
        }

        if (!string.IsNullOrEmpty(pluginOptions.Release))
        {
            options.Release = pluginOptions.Release;
        }

        if (!string.IsNullOrEmpty(pluginOptions.ServerName))
        {
            options.ServerName = pluginOptions.ServerName;
        }

        options.SendDefaultPii = pluginOptions.SendDefaultPii;
        options.SampleRate = (float)pluginOptions.SampleRate;
        options.MaxBreadcrumbs = pluginOptions.MaxBreadcrumbs;
        options.AttachStacktrace = pluginOptions.AttachStacktrace;
        options.ShutdownTimeout = pluginOptions.ShutdownTimeout;

        // Tracing
        if (pluginOptions.Tracing.Enabled)
        {
            options.TracesSampleRate = pluginOptions.Tracing.SampleRate;

            // Configure traces sampler for URL filtering
            if (pluginOptions.Tracing.IgnoreUrls.Count > 0)
            {
                options.TracesSampler = context =>
                {
                    var transactionName = context.TransactionContext.Name;
                    foreach (var pattern in pluginOptions.Tracing.IgnoreUrls)
                    {
                        if (MatchesPattern(transactionName, pattern))
                        {
                            return 0.0;
                        }
                    }
                    return pluginOptions.Tracing.SampleRate;
                };
            }
        }

        // Profiling
        if (pluginOptions.Profiling.Enabled)
        {
            options.ProfilesSampleRate = pluginOptions.Profiling.SampleRate;
        }

        // In-app settings
        foreach (var include in pluginOptions.InAppInclude)
        {
            options.AddInAppInclude(include);
        }

        foreach (var exclude in pluginOptions.InAppExclude)
        {
            options.AddInAppExclude(exclude);
        }

        // Exception filtering
        foreach (var exceptionType in pluginOptions.Filtering.IgnoreExceptionTypes)
        {
            var type = Type.GetType(exceptionType);
            if (type is not null && typeof(Exception).IsAssignableFrom(type))
            {
                options.AddExceptionFilter(new TypeExceptionFilter(type));
            }
        }

        // BeforeSend for message filtering
        if (pluginOptions.Filtering.IgnoreMessages.Count > 0)
        {
            options.SetBeforeSend((sentryEvent, hint) =>
            {
                // Filter by message
                if (sentryEvent.Message?.Formatted is not null)
                {
                    foreach (var pattern in pluginOptions.Filtering.IgnoreMessages)
                    {
                        if (sentryEvent.Message.Formatted.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            return null;
                        }
                    }
                }

                return sentryEvent;
            });
        }
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (pattern.EndsWith('*'))
        {
            return value.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith('*'))
        {
            return value.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase);
        }

        return value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static SentryLevel MapDiagnosticLevel(DiagnosticLevel level) => level switch
    {
        DiagnosticLevel.Debug => SentryLevel.Debug,
        DiagnosticLevel.Info => SentryLevel.Info,
        DiagnosticLevel.Warning => SentryLevel.Warning,
        DiagnosticLevel.Error => SentryLevel.Error,
        DiagnosticLevel.Fatal => SentryLevel.Fatal,
        _ => SentryLevel.Warning
    };
}

/// <summary>
/// Exception filter that filters exceptions by type.
/// </summary>
internal sealed class TypeExceptionFilter : IExceptionFilter
{
    private readonly Type _exceptionType;

    public TypeExceptionFilter(Type exceptionType)
    {
        _exceptionType = exceptionType;
    }

    public bool Filter(Exception ex)
    {
        return _exceptionType.IsInstanceOfType(ex);
    }
}
#endif
