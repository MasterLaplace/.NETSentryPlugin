#if ASPNET_CLASSIC
using System.Web;
using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Enrichers;

/// <summary>
/// Enriches events with HTTP request information for ASP.NET Classic.
/// </summary>
public sealed class AspNetRequestEnricher : IEventEnricher
{
    /// <inheritdoc/>
    public int Order => 100;

    /// <inheritdoc/>
    public void Enrich(EventEnrichmentContext context)
    {
        var httpContext = HttpContext.Current;
        if (httpContext is null)
        {
            return;
        }

        var request = httpContext.Request;

        context.SetTag("http.method", request.HttpMethod);
        context.SetTag("http.url", request.Url?.AbsoluteUri ?? "/");

        if (!string.IsNullOrEmpty(request.QueryString.ToString()))
        {
            context.SetExtra("http.query_string", request.QueryString.ToString());
        }

        context.SetContext("Request", new
        {
            Method = request.HttpMethod,
            Url = request.Url?.AbsoluteUri,
            QueryString = request.QueryString.ToString(),
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            UserAgent = request.UserAgent
        });

        context.SetTag("http.status_code", httpContext.Response.StatusCode.ToString());
    }
}
#endif
