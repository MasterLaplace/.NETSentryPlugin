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
    /// Adds a namespace prefix to include as "in-app" code.
    /// Alias for IncludeInApp for compatibility.
    /// </summary>
    /// <param name="prefix">The namespace prefix.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder AddInAppInclude(string prefix) => IncludeInApp(prefix);

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
    /// Adds a tag that will be applied to all events.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder AddTag(string key, string value)
    {
        _options.DefaultTags[key] = value;
        return this;
    }

    /// <summary>
    /// Applies additional configuration to the options.
    /// </summary>
    /// <param name="configure">Action to configure options.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder Configure(Action<SentryPluginOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _configurationActions.Add(configure);
        return this;
    }

    /// <summary>
    /// Sets a callback to be invoked before events are sent to Sentry.
    /// </summary>
    /// <param name="beforeSend">The callback. Return false to discard the event.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// Use this callback to scrub sensitive data, add custom context, or filter events.
    /// <example>
    /// <code>
    /// builder.SetBeforeSend(info =>
    /// {
    ///     // Scrub sensitive data
    ///     if (info.Message?.Contains("password") == true)
    ///     {
    ///         info.Message = "[REDACTED]";
    ///     }
    ///
    ///     // Add custom tag
    ///     info.Tags["custom"] = "value";
    ///
    ///     // Return true to send, false to discard
    ///     return true;
    /// });
    /// </code>
    /// </example>
    /// </remarks>
    public SentryPluginBuilder SetBeforeSend(SentryCallbacks.BeforeSendCallback beforeSend)
    {
        ArgumentNullException.ThrowIfNull(beforeSend);
        _options.BeforeSend = beforeSend;
        return this;
    }

    /// <summary>
    /// Sets a callback to be invoked before breadcrumbs are captured.
    /// </summary>
    /// <param name="beforeBreadcrumb">The callback. Return false to discard the breadcrumb.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// Use this callback to filter noisy breadcrumbs or scrub sensitive data.
    /// <example>
    /// <code>
    /// builder.SetBeforeBreadcrumb(info =>
    /// {
    ///     // Filter out debug breadcrumbs
    ///     if (info.Level == PluginBreadcrumbLevel.Debug)
    ///         return false;
    ///
    ///     // Scrub sensitive data
    ///     if (info.Message?.Contains("secret") == true)
    ///         info.Message = "[REDACTED]";
    ///
    ///     return true;
    /// });
    /// </code>
    /// </example>
    /// </remarks>
    public SentryPluginBuilder SetBeforeBreadcrumb(SentryCallbacks.BeforeBreadcrumbCallback beforeBreadcrumb)
    {
        ArgumentNullException.ThrowIfNull(beforeBreadcrumb);
        _options.BeforeBreadcrumb = beforeBreadcrumb;
        return this;
    }

    /// <summary>
    /// Sets a dynamic transaction sampler callback.
    /// </summary>
    /// <param name="tracesSampler">The sampler callback. Return a rate (0.0-1.0) or null for default.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// This is mutually exclusive with the static TracesSampleRate.
    /// Use this for dynamic sampling based on transaction context.
    /// <example>
    /// <code>
    /// builder.SetTracesSampler(context =>
    /// {
    ///     // Don't sample health checks
    ///     if (context.TransactionName.Contains("/health"))
    ///         return 0.0;
    ///
    ///     // Higher rate for payment operations
    ///     if (context.TransactionName.Contains("/payment"))
    ///         return 1.0;
    ///
    ///     // Inherit parent decision for distributed tracing
    ///     if (context.ParentSampled.HasValue)
    ///         return context.ParentSampled.Value ? 1.0 : 0.0;
    ///
    ///     // Default rate
    ///     return 0.2;
    /// });
    /// </code>
    /// </example>
    /// </remarks>
    public SentryPluginBuilder SetTracesSampler(SentryCallbacks.TracesSamplerCallback tracesSampler)
    {
        ArgumentNullException.ThrowIfNull(tracesSampler);
        _options.TracesSampler = tracesSampler;
        return this;
    }

    /// <summary>
    /// Configures data scrubbing options for sensitive data handling.
    /// </summary>
    /// <param name="configure">Action to configure data scrubbing options.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureDataScrubbing(scrubbing =>
    /// {
    ///     scrubbing.SensitiveFields.Add("customSecret");
    ///     scrubbing.SensitivePatterns.Add(@"\bAPI-[A-Z0-9]{32}\b");
    /// });
    /// </code>
    /// </example>
    public SentryPluginBuilder ConfigureDataScrubbing(Action<DataScrubbingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_options.DataScrubbing);
        return this;
    }

    /// <summary>
    /// Enables capture of response headers in error events.
    /// </summary>
    /// <param name="enabled">Whether to capture response headers.</param>
    /// <returns>The builder for chaining.</returns>
    public SentryPluginBuilder CaptureResponseHeaders(bool enabled = true)
    {
        _options.CaptureResponseHeaders = enabled;
        return this;
    }

    /// <summary>
    /// Sets the request body capture size.
    /// </summary>
    /// <param name="size">The maximum size of request bodies to capture.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// Request body capture requires <c>SendDefaultPii</c> to be enabled for full functionality.
    /// <list type="bullet">
    ///   <item><description><c>None</c> - Don't capture request bodies</description></item>
    ///   <item><description><c>Small</c> - Capture up to 1KB</description></item>
    ///   <item><description><c>Medium</c> - Capture up to 10KB</description></item>
    ///   <item><description><c>Always</c> - Always capture (up to SDK limit)</description></item>
    /// </list>
    /// </remarks>
    public SentryPluginBuilder WithMaxRequestBodySize(RequestBodySize size)
    {
        _options.MaxRequestBodySize = size;
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
