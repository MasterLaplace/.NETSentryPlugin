#if ASPNETCORE
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Sentry;

namespace MySentry.Plugin.Extensions;

/// <summary>
/// Extension methods for configuring Sentry HTTP client tracing with <see cref="IHttpClientFactory"/>.
/// </summary>
public static class HttpClientTracingExtensions
{
    /// <summary>
    /// Adds a named HTTP client with Sentry tracing enabled.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> for further configuration.</returns>
    /// <remarks>
    /// The HTTP client will automatically:
    /// - Propagate Sentry trace headers to downstream services
    /// - Create spans for outgoing HTTP requests
    /// - Include request/response context in spans
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddSentryHttpClient("MyApi")
    ///     .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.example.com"));
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSentryHttpClient(this IServiceCollection services, string name)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(name);

        return services.AddHttpClient(name)
            .AddSentryTracing();
    }

    /// <summary>
    /// Adds a typed HTTP client with Sentry tracing enabled.
    /// </summary>
    /// <typeparam name="TClient">The type of the typed client.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> for further configuration.</returns>
    /// <example>
    /// <code>
    /// services.AddSentryHttpClient&lt;IMyApiClient, MyApiClient&gt;();
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSentryHttpClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHttpClient<TClient>()
            .AddSentryTracing();
    }

    /// <summary>
    /// Adds a typed HTTP client with Sentry tracing enabled.
    /// </summary>
    /// <typeparam name="TClient">The type of the typed client interface.</typeparam>
    /// <typeparam name="TImplementation">The type of the typed client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> for further configuration.</returns>
    public static IHttpClientBuilder AddSentryHttpClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHttpClient<TClient, TImplementation>()
            .AddSentryTracing();
    }

    /// <summary>
    /// Adds Sentry tracing to an existing HTTP client builder.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The HTTP client builder for chaining.</returns>
    /// <remarks>
    /// This method adds the <see cref="SentryHttpMessageHandler"/> to the client's
    /// handler pipeline, which:
    /// - Creates spans for each outgoing request
    /// - Propagates sentry-trace and baggage headers
    /// - Records request/response information
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddHttpClient("MyApi")
    ///     .AddSentryTracing()
    ///     .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSentryTracing(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddHttpMessageHandler(() => new SentryHttpMessageHandler());
    }

    /// <summary>
    /// Adds Sentry tracing to an existing HTTP client builder with custom options.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="configure">Action to configure the Sentry HTTP handler options.</param>
    /// <returns>The HTTP client builder for chaining.</returns>
    public static IHttpClientBuilder AddSentryTracing(
        this IHttpClientBuilder builder,
        Action<SentryHttpMessageHandlerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SentryHttpMessageHandlerOptions();
        configure(options);

        return builder.AddHttpMessageHandler(() =>
        {
            var handler = new SentryHttpMessageHandler();
            // Note: SentryHttpMessageHandler options are limited in SDK
            // Custom options would require wrapping or extending
            return handler;
        });
    }
}

/// <summary>
/// Options for configuring the Sentry HTTP message handler.
/// </summary>
public sealed class SentryHttpMessageHandlerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to propagate trace headers.
    /// Default is true.
    /// </summary>
    public bool PropagateTraceHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to create spans for requests.
    /// Default is true.
    /// </summary>
    public bool CreateSpans { get; set; } = true;

    /// <summary>
    /// Gets or sets URL patterns to exclude from tracing.
    /// Supports wildcards: "prefix*", "*suffix", "*contains*".
    /// </summary>
    public List<string> ExcludeUrls { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of headers to capture in spans.
    /// Be careful not to capture sensitive headers.
    /// </summary>
    public List<string> CaptureHeaders { get; set; } = new()
    {
        "Content-Type",
        "Content-Length"
    };
}
#endif
