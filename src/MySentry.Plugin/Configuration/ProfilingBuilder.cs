namespace MySentry.Plugin.Configuration;

/// <summary>
/// Fluent builder for profiling configuration.
/// </summary>
public sealed class ProfilingBuilder
{
    private readonly ProfilingOptions _options;

    internal ProfilingBuilder(ProfilingOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets the profile sample rate.
    /// </summary>
    /// <param name="sampleRate">The sample rate (0.0 to 1.0).</param>
    /// <returns>The builder for chaining.</returns>
    public ProfilingBuilder WithSampleRate(double sampleRate)
    {
    #if NET5_0_OR_GREATER
        _options.SampleRate = Math.Clamp(sampleRate, 0.0, 1.0);
    #else
        _options.SampleRate = MySentry.Plugin.NetFrameworkPolyfills.Clamp(sampleRate, 0.0, 1.0);
    #endif
        return this;
    }

    /// <summary>
    /// Sets the startup timeout for the profiler.
    /// </summary>
    /// <param name="timeout">The startup timeout.</param>
    /// <returns>The builder for chaining.</returns>
    public ProfilingBuilder WithStartupTimeout(TimeSpan timeout)
    {
        _options.StartupTimeout = timeout;
        return this;
    }
}
