namespace MySentry.Plugin.Configuration;

/// <summary>
/// Fluent builder for tracing configuration.
/// </summary>
public sealed class TracingBuilder
{
    private readonly TracingOptions _options;

    internal TracingBuilder(TracingOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets the trace sample rate.
    /// </summary>
    /// <param name="sampleRate">The sample rate (0.0 to 1.0).</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder WithSampleRate(double sampleRate)
    {
        _options.SampleRate = Math.Clamp(sampleRate, 0.0, 1.0);
        return this;
    }

    /// <summary>
    /// Enables or disables tracing of all incoming requests.
    /// </summary>
    /// <param name="enabled">Whether to trace all requests.</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder TraceAllRequests(bool enabled = true)
    {
        _options.TraceAllRequests = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables tracing of database operations.
    /// </summary>
    /// <param name="enabled">Whether to trace database operations.</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder TraceDatabase(bool enabled = true)
    {
        _options.TraceDatabase = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables tracing of outgoing HTTP requests.
    /// </summary>
    /// <param name="enabled">Whether to trace HTTP clients.</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder TraceHttpClients(bool enabled = true)
    {
        _options.TraceHttpClients = enabled;
        return this;
    }

    /// <summary>
    /// Adds URL patterns to ignore when tracing.
    /// </summary>
    /// <param name="patterns">The URL patterns to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder IgnoreUrls(params string[] patterns)
    {
        _options.IgnoreUrls.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Clears the default ignored URLs and sets new ones.
    /// </summary>
    /// <param name="patterns">The URL patterns to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder SetIgnoreUrls(params string[] patterns)
    {
        _options.IgnoreUrls.Clear();
        _options.IgnoreUrls.AddRange(patterns);
        return this;
    }

    /// <summary>
    /// Adds transaction names to ignore.
    /// </summary>
    /// <param name="names">The transaction names to ignore.</param>
    /// <returns>The builder for chaining.</returns>
    public TracingBuilder IgnoreTransactions(params string[] names)
    {
        _options.IgnoreTransactions.AddRange(names);
        return this;
    }
}
