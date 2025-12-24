using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Configuration;
using MySentry.Plugin.Enrichers;

namespace MySentry.Plugin.Middleware;

/// <summary>
/// ASP.NET Core middleware that integrates MySentry error capturing and performance monitoring.
/// </summary>
public sealed class MySentryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MySentryMiddleware> _logger;
    private readonly SentryPluginOptions _options;
    private readonly IEnumerable<IEventEnricher> _enrichers;

    /// <summary>
    /// Creates a new instance of the MySentry middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The plugin options.</param>
    /// <param name="enrichers">The event enrichers.</param>
    public MySentryMiddleware(
        RequestDelegate next,
        ILogger<MySentryMiddleware> logger,
        IOptions<SentryPluginOptions> options,
        IEnumerable<IEventEnricher> enrichers)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
        _enrichers = enrichers.OrderBy(e => e.Order);
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="sentryPlugin">The Sentry plugin.</param>
    /// <returns>A task representing the middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context, ISentryPlugin sentryPlugin)
    {
        if (ShouldIgnoreRequest(context))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var requestPath = context.Request.Path.Value ?? "/";
        var requestMethod = context.Request.Method;

        sentryPlugin.AddBreadcrumb(
            $"{requestMethod} {requestPath}",
            "http",
            "http.request",
            PluginBreadcrumbLevel.Info);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context).ConfigureAwait(false);

            stopwatch.Stop();

            // Add response breadcrumb
            sentryPlugin.AddBreadcrumb(
                $"{requestMethod} {requestPath} -> {context.Response.StatusCode}",
                "http",
                "http.response",
                new Dictionary<string, string>
                {
                    ["status_code"] = context.Response.StatusCode.ToString(),
                    ["duration_ms"] = stopwatch.ElapsedMilliseconds.ToString()
                },
                context.Response.StatusCode >= 400 ? PluginBreadcrumbLevel.Warning : PluginBreadcrumbLevel.Info);

            // Check for error status codes
            if (ShouldCaptureStatusCode(context.Response.StatusCode))
            {
                var message = $"HTTP {context.Response.StatusCode} - {requestMethod} {requestPath}";
                sentryPlugin.CaptureMessage(message, PluginSeverityLevel.Warning, scope =>
                {
                    scope.SetTag("http.status_code", context.Response.StatusCode.ToString());
                    scope.SetTag("http.method", requestMethod);
                    scope.SetTag("http.path", requestPath);
                });
            }
        }
        catch (Exception ex) when (!IsIgnoredException(ex))
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Unhandled exception in request {Method} {Path}", requestMethod, requestPath);

            sentryPlugin.CaptureException(ex, scope =>
            {
                scope.SetTag("http.method", requestMethod);
                scope.SetTag("http.path", requestPath);
                scope.SetExtra("request.duration_ms", stopwatch.ElapsedMilliseconds);

                // Apply enrichers
                var enrichmentContext = new EventEnrichmentContext(ex, PluginSeverityLevel.Error);
                foreach (var enricher in _enrichers)
                {
                    try
                    {
                        enricher.Enrich(enrichmentContext);
                    }
                    catch (Exception enricherEx)
                    {
                        _logger.LogWarning(enricherEx, "Enricher {EnricherType} failed", enricher.GetType().Name);
                    }
                }

                // Apply enrichment results
                foreach (var tag in enrichmentContext.Tags)
                {
                    scope.SetTag(tag.Key, tag.Value);
                }

                foreach (var extra in enrichmentContext.Extras)
                {
                    scope.SetExtra(extra.Key, extra.Value);
                }

                foreach (var ctx in enrichmentContext.Contexts)
                {
                    scope.SetContext(ctx.Key, ctx.Value);
                }

                if (enrichmentContext.User is not null)
                {
                    scope.SetUser(enrichmentContext.User);
                }
            });

            throw;
        }
    }

    private bool ShouldIgnoreRequest(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        foreach (var pattern in _options.Filtering.IgnoreUrls)
        {
            if (MatchesPattern(path, pattern))
            {
                return true;
            }
        }

        // Check user agent
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            foreach (var pattern in _options.Filtering.IgnoreUserAgents)
            {
                if (MatchesPattern(userAgent, pattern))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool ShouldCaptureStatusCode(int statusCode)
    {
        if (statusCode < 400)
        {
            return false;
        }

        return !_options.Filtering.IgnoreStatusCodes.Contains(statusCode);
    }

    private bool IsIgnoredException(Exception ex)
    {
        var exceptionTypeName = ex.GetType().FullName;
        return exceptionTypeName is not null &&
               _options.Filtering.IgnoreExceptionTypes.Contains(exceptionTypeName);
    }

    private static bool MatchesPattern(string value, string pattern)
    {
        if (pattern.EndsWith('*'))
        {
            var prefix = pattern[..^1];
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith('*'))
        {
            var suffix = pattern[1..];
            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.Contains('*'))
        {
            var parts = pattern.Split('*');
            if (parts.Length == 2)
            {
                return value.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase) &&
                       value.EndsWith(parts[1], StringComparison.OrdinalIgnoreCase);
            }
        }

        return value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
