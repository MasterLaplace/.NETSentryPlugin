using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MySentry.Plugin.Enrichers;

/// <summary>
/// Enriches events with environment and runtime information.
/// </summary>
public sealed class EnvironmentEnricher : IEventEnricher
{
    /// <inheritdoc/>
    public int Order => 50;

    /// <inheritdoc/>
    public void Enrich(EventEnrichmentContext context)
    {
        context.SetContext("Runtime", new
        {
            Name = RuntimeInformation.FrameworkDescription,
            Version = Environment.Version.ToString(),
            OSDescription = RuntimeInformation.OSDescription,
            OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString()
        });

        context.SetContext("App", new
        {
            Name = Assembly.GetEntryAssembly()?.GetName().Name,
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
#if NET5_0_OR_GREATER
            ProcessId = Environment.ProcessId,
#else
            ProcessId = MySentry.Plugin.NetFrameworkPolyfills.GetProcessId(),
#endif
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            CurrentDirectory = Environment.CurrentDirectory
        });

        context.SetTag("runtime", RuntimeInformation.FrameworkDescription);
        context.SetTag("os", RuntimeInformation.OSDescription);
    }
}
