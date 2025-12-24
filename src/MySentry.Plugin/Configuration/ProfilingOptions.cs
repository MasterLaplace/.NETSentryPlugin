namespace MySentry.Plugin.Configuration;

/// <summary>
/// Configuration options for profiling.
/// </summary>
public sealed class ProfilingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether profiling is enabled.
    /// Requires .NET 8.0 or later on supported platforms.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the sample rate for profiles (0.0 to 1.0).
    /// This is relative to the traces sample rate.
    /// </summary>
    public double SampleRate { get; set; } = SamplingRates.Disabled;

    /// <summary>
    /// Gets or sets the startup timeout for the profiler.
    /// Only applicable on Windows, Linux, and macOS.
    /// </summary>
    public TimeSpan? StartupTimeout { get; set; }
}
