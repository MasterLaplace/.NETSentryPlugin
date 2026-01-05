#if ASPNETCORE
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Configuration;
using MySentry.Plugin.Utilities;

namespace MySentry.Plugin.Middleware;

/// <summary>
/// Middleware that automatically creates performance transactions for HTTP requests.
/// </summary>
public sealed class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;
    private readonly SentryPluginOptions _options;

    /// <summary>
    /// Creates a new instance of the performance middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The plugin options.</param>
    public PerformanceMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMiddleware> logger,
        IOptions<SentryPluginOptions> options)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="sentryPlugin">The Sentry plugin.</param>
    /// <returns>A task representing the middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context, ISentryPlugin sentryPlugin)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(sentryPlugin);

        if (!_options.Tracing.Enabled || !_options.Tracing.TraceAllRequests)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var requestPath = context.Request.Path.Value ?? "/";
        if (PatternMatcher.MatchesAny(requestPath, _options.Tracing.IgnoreUrls))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var transactionName = $"{context.Request.Method} {GetTransactionName(context)}";
        using var transaction = sentryPlugin.StartTransaction(transactionName, "http.server");

        transaction.SetTag("http.method", context.Request.Method);
        transaction.SetTag("http.url", requestPath);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context).ConfigureAwait(false);

            stopwatch.Stop();

            transaction.SetHttpStatus(context.Response.StatusCode);
            transaction.SetExtra("duration_ms", stopwatch.ElapsedMilliseconds);

            var status = context.Response.StatusCode switch
            {
                >= 200 and < 300 => PluginSpanStatus.Ok,
                >= 400 and < 500 => PluginSpanStatus.InvalidArgument,
                >= 500 => PluginSpanStatus.InternalError,
                _ => PluginSpanStatus.Unknown
            };

            transaction.Finish(status);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            transaction.SetExtra("duration_ms", stopwatch.ElapsedMilliseconds);
            transaction.SetTag("error", "true");
            transaction.SetExtra("exception.type", ex.GetType().Name);
            transaction.SetExtra("exception.message", ex.Message);

            transaction.Finish(ex);
            throw;
        }
    }

    private static string GetTransactionName(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            return routeEndpoint.RoutePattern.RawText ?? context.Request.Path.Value ?? "/";
        }

        return context.Request.Path.Value ?? "/";
    }
}
#endif
