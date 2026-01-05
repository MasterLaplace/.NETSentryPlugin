#if ASPNETCORE
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MySentry.Plugin.Abstractions;
using Sentry;

namespace MySentry.Plugin.Features.Breadcrumbs;

/// <summary>
/// DelegatingHandler that automatically creates breadcrumbs for outgoing HTTP requests.
/// </summary>
public sealed class HttpClientBreadcrumbHandler : DelegatingHandler
{
    private readonly ISentryPlugin? _plugin;

    /// <summary>
    /// Creates a new instance of the HTTP client breadcrumb handler.
    /// Uses the static SentrySdk for breadcrumb capture.
    /// </summary>
    public HttpClientBreadcrumbHandler()
    {
        _plugin = null;
    }

    /// <summary>
    /// Creates a new instance of the HTTP client breadcrumb handler with plugin reference.
    /// </summary>
    /// <param name="plugin">The Sentry plugin to use for breadcrumb capture.</param>
    public HttpClientBreadcrumbHandler(ISentryPlugin plugin)
    {
        _plugin = plugin;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage? response = null;
        Exception? exception = null;

        try
        {
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            CaptureBreadcrumb(request, response, stopwatch.ElapsedMilliseconds, exception);
        }
    }

    private void CaptureBreadcrumb(
        HttpRequestMessage request,
        HttpResponseMessage? response,
        long durationMs,
        Exception? exception)
    {
        var uri = request.RequestUri;
        var data = new Dictionary<string, string>
        {
            ["method"] = request.Method.Method,
            ["url"] = uri?.ToString() ?? "unknown",
            ["duration_ms"] = durationMs.ToString()
        };

        if (uri is not null)
        {
            data["host"] = uri.Host;
            data["path"] = uri.AbsolutePath;
        }

        if (response is not null)
        {
            data["status_code"] = ((int)response.StatusCode).ToString();
            data["reason"] = response.ReasonPhrase ?? string.Empty;
        }

        if (exception is not null)
        {
            data["error"] = exception.GetType().Name;
            data["error_message"] = exception.Message;
        }

        var message = response is not null
            ? $"HTTP {request.Method} {uri?.AbsolutePath ?? "/"} ({(int)response.StatusCode})"
            : $"HTTP {request.Method} {uri?.AbsolutePath ?? "/"} (failed)";

        var level = GetBreadcrumbLevel(response, exception);

        if (_plugin is not null)
        {
            _plugin.Breadcrumbs.AddBreadcrumb(
                message,
                "http.client",
                "http",
                data,
                level);
        }
        else
        {
            SentrySdk.AddBreadcrumb(
                message,
                "http.client",
                "http",
                data,
                MapToSentryLevel(level));
        }
    }

    private static PluginBreadcrumbLevel GetBreadcrumbLevel(
        HttpResponseMessage? response,
        Exception? exception)
    {
        if (exception is not null)
        {
            return PluginBreadcrumbLevel.Error;
        }

        if (response is null)
        {
            return PluginBreadcrumbLevel.Warning;
        }

        var statusCode = (int)response.StatusCode;
        return statusCode switch
        {
            >= 500 => PluginBreadcrumbLevel.Error,
            >= 400 => PluginBreadcrumbLevel.Warning,
            _ => PluginBreadcrumbLevel.Info
        };
    }

    private static Sentry.BreadcrumbLevel MapToSentryLevel(PluginBreadcrumbLevel level) => level switch
    {
        PluginBreadcrumbLevel.Debug => Sentry.BreadcrumbLevel.Debug,
        PluginBreadcrumbLevel.Info => Sentry.BreadcrumbLevel.Info,
        PluginBreadcrumbLevel.Warning => Sentry.BreadcrumbLevel.Warning,
        PluginBreadcrumbLevel.Error => Sentry.BreadcrumbLevel.Error,
        PluginBreadcrumbLevel.Fatal => Sentry.BreadcrumbLevel.Fatal,
        _ => Sentry.BreadcrumbLevel.Info
    };
}

/// <summary>
/// Extension methods for registering HTTP client breadcrumb handlers.
/// </summary>
public static class HttpClientBreadcrumbExtensions
{
    /// <summary>
    /// Adds automatic breadcrumb capture to an HTTP client builder.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The HTTP client builder for chaining.</returns>
    public static IHttpClientBuilder AddBreadcrumbCapture(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddHttpMessageHandler(() => new HttpClientBreadcrumbHandler());
    }

    /// <summary>
    /// Adds automatic breadcrumb capture using the MySentry plugin.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="serviceProvider">The service provider to resolve the plugin from.</param>
    /// <returns>The HTTP client builder for chaining.</returns>
    public static IHttpClientBuilder AddBreadcrumbCapture(
        this IHttpClientBuilder builder,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var plugin = serviceProvider.GetService<ISentryPlugin>();
        return builder.AddHttpMessageHandler(() =>
            plugin is not null
                ? new HttpClientBreadcrumbHandler(plugin)
                : new HttpClientBreadcrumbHandler());
    }
}
#endif
