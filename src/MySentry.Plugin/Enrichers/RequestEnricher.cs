#if ASPNETCORE
using Microsoft.AspNetCore.Http;
using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Enrichers;

/// <summary>
/// Enriches events with HTTP request information.
/// </summary>
public sealed class RequestEnricher : IEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates a new instance of the request enricher.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public RequestEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public int Order => 100;

    /// <inheritdoc/>
    public void Enrich(EventEnrichmentContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        var request = httpContext.Request;

        context.SetTag("http.method", request.Method);
        context.SetTag("http.url", $"{request.Scheme}://{request.Host}{request.Path}");

        if (request.QueryString.HasValue)
        {
            context.SetExtra("http.query_string", request.QueryString.Value);
        }

        context.SetContext("Request", new
        {
            Method = request.Method,
            Url = $"{request.Scheme}://{request.Host}{request.Path}",
            QueryString = request.QueryString.HasValue ? request.QueryString.Value : null,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            Protocol = request.Protocol
        });

        if (httpContext.Response.HasStarted)
        {
            context.SetTag("http.status_code", httpContext.Response.StatusCode.ToString());
        }
    }
}
#endif
