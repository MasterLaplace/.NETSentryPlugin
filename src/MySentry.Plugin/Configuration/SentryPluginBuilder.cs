using MySentry.Plugin.Enrichers;

namespace MySentry.Plugin.Configuration;

/// <summary>
/// Fluent builder for configuring the MySentry plugin.
/// Provides a clean, discoverable API for setting up Sentry integration.
/// </summary>
public sealed class SentryPluginBuilder
{
    private readonly SentryPluginOptions _options = new();
    private readonly List<Type> _enricherTypes = new();
    private readonly List<IEventEnricher> _enricherInstances = new();
    private readonly List<Action<SentryPluginOptions>> _configurationActions = new();

    /// <summary>
    /// Gets the configured options (read-only access for validation and testing).
    /// </summary>
    public SentryPluginOptions Options => _options;

    /// <summary>
    /// Gets the registered enricher types.
    /// </summary>
    internal IReadOnlyList<Type> EnricherTypes => _enricherTypes;

    /// <summary>
    /// Gets the registered enricher instances.
    /// </summary>
    internal IReadOnlyList<IEventEnricher> EnricherInstances => _enricherInstances;

    /// <summary>
    /// Sets the Sentry DSN (Data Source Name).
    /// </summary>
    /// <param name="dsn">The Sentry DSN.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithDsn(string dsn)
    {
        _options.Dsn = dsn;
        return this;
    }

    /// <summary>
    /// Sets the environment name.
    /// </summary>
    /// <param name="environment">The environment name.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithEnvironment(string environment)
    {
        _options.Environment = environment;
        return this;
    }

    /// <summary>
    /// Sets the release version.
    /// </summary>
    /// <param name="release">The release version.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithRelease(string release)
    {
        _options.Release = release;
        return this;
    }

    /// <summary>
    /// Sets the server name.
    /// </summary>
    /// <param name="serverName">The server name.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithServerName(string serverName)
    {
        _options.ServerName = serverName;
        return this;
    }

    /// <summary>
    /// Enables or disables debug mode.
    /// </summary>
    /// <param name="enabled">Whether debug mode is enabled.</param>
    /// <param name="level">The diagnostic level when debug is enabled.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithDebug(bool enabled = true, DiagnosticLevel level = DiagnosticLevel.Warning)
    {
        _options.Debug = enabled;
        _options.DiagnosticLevel = level;
        return this;
    }

    /// <summary>
    /// Enables sending of default PII (personally identifiable information).
    /// </summary>
    /// <param name="enabled">Whether to send default PII.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithDefaultPii(bool enabled = true)
    {
        _options.SendDefaultPii = enabled;
        return this;
    }

    /// <summary>
    /// Sets the error sample rate.
    /// </summary>
    /// <param name="sampleRate">The sample rate (0.0 to 1.0).</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithSampleRate(double sampleRate)
    {
    #if NET5_0_OR_GREATER
        _options.SampleRate = Math.Clamp(sampleRate, 0.0, 1.0);
    #else
        _options.SampleRate = MySentry.Plugin.NetFrameworkPolyfills.Clamp(sampleRate, 0.0, 1.0);
    #endif
        return this;
    }

    /// <summary>
    /// Sets the maximum number of breadcrumbs to capture.
    /// </summary>
    /// <param name="maxBreadcrumbs">The maximum number of breadcrumbs.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithMaxBreadcrumbs(int maxBreadcrumbs)
    {
        _options.MaxBreadcrumbs = Math.Max(0, maxBreadcrumbs);
        return this;
    }

    /// <summary>
    /// Enables or disables stack trace attachment.
    /// </summary>
    /// <param name="enabled">Whether to attach stack traces.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithStackTrace(bool enabled = true)
    {
        _options.AttachStacktrace = enabled;
        return this;
    }

    /// <summary>
    /// Sets the shutdown timeout.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithShutdownTimeout(TimeSpan timeout)
    {
        _options.ShutdownTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the request body capture size.
    /// </summary>
    /// <param name="size">The maximum request body size to capture.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithRequestBodySize(RequestBodySize size)
    {
        _options.MaxRequestBodySize = size;
        return this;
    }

    /// <summary>
    /// Enables structured logs capture.
    /// </summary>
    /// <param name="enabled">Whether to capture structured logs.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder WithLogs(bool enabled = true)
    {
        _options.EnableLogs = enabled;
        return this;
    }

    /// <summary>
    /// Configures tracing options.
    /// </summary>
    /// <param name="configure">Action to configure tracing options.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder EnableTracing(Action<TracingBuilder>? configure = null)
    {
        _options.Tracing.Enabled = true;
        var builder = new TracingBuilder(_options.Tracing);
        configure?.Invoke(builder);
        return this;
    }

    /// <summary>
    /// Enables tracing with a specific sample rate.
    /// </summary>
    /// <param name="sampleRate">The trace sample rate (0.0 to 1.0).</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder EnableTracing(double sampleRate)
    {
        _options.Tracing.Enabled = true;
    #if NET5_0_OR_GREATER
        _options.Tracing.SampleRate = Math.Clamp(sampleRate, 0.0, 1.0);
    #else
        _options.Tracing.SampleRate = MySentry.Plugin.NetFrameworkPolyfills.Clamp(sampleRate, 0.0, 1.0);
    #endif
        return this;
    }

    /// <summary>
    /// Configures profiling options.
    /// </summary>
    /// <param name="configure">Action to configure profiling options.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder EnableProfiling(Action<ProfilingBuilder>? configure = null)
    {
        _options.Profiling.Enabled = true;
        var builder = new ProfilingBuilder(_options.Profiling);
        configure?.Invoke(builder);
        return this;
    }

    /// <summary>
    /// Enables profiling with a specific sample rate.
    /// </summary>
    /// <param name="sampleRate">The profile sample rate (0.0 to 1.0).</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder EnableProfiling(double sampleRate)
    {
        _options.Profiling.Enabled = true;
    #if NET5_0_OR_GREATER
        _options.Profiling.SampleRate = Math.Clamp(sampleRate, 0.0, 1.0);
    #else
        _options.Profiling.SampleRate = MySentry.Plugin.NetFrameworkPolyfills.Clamp(sampleRate, 0.0, 1.0);
    #endif
        return this;
    }

    /// <summary>
    /// Configures event filtering options.
    /// </summary>
    /// <param name="configure">Action to configure filtering options.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder FilterEvents(Action<FilteringBuilder> configure)
    {
        var builder = new FilteringBuilder(_options.Filtering);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Adds an event enricher by type.
    /// The enricher will be resolved from the DI container.
    /// </summary>
    /// <typeparam name="TEnricher">The enricher type.</typeparam>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder EnrichWith<TEnricher>() where TEnricher : class, IEventEnricher
    {
        _enricherTypes.Add(typeof(TEnricher));
        return this;
    }

    /// <summary>
    /// Adds an event enricher instance.
    /// </summary>
    /// <param name="enricher">The enricher instance.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder EnrichWith(IEventEnricher enricher)
    {
        _enricherInstances.Add(enricher);
        return this;
    }

    /// <summary>
    /// Adds a namespace prefix to include as "in-app" code.
    /// </summary>
    /// <param name="prefix">The namespace prefix.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder IncludeInApp(string prefix)
    {
        _options.InAppInclude.Add(prefix);
        return this;
    }

    /// <summary>
    /// Adds a namespace prefix to exclude from "in-app" code.
    /// </summary>
    /// <param name="prefix">The namespace prefix.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder ExcludeFromInApp(string prefix)
    {
        _options.InAppExclude.Add(prefix);
        return this;
    }

    /// <summary>
    /// Applies additional configuration to the options.
    /// </summary>
    /// <param name="configure">Action to configure options.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder Configure(Action<SentryPluginOptions> configure)
    {
        _configurationActions.Add(configure);
        return this;
    }

    /// <summary>
    /// Builds the final options by applying all configuration actions.
    /// </summary>
    internal void ApplyConfiguration()
    {
        foreach (var action in _configurationActions)
        {
            action(_options);
        }
    }
}
