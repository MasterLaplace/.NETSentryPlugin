#if ASPNETCORE
using Microsoft.AspNetCore.Http;
using MySentry.Plugin.Abstractions;

namespace MySentry.Plugin.Enrichers;

/// <summary>
/// Enriches events with user information from the HTTP context.
/// </summary>
public sealed class UserEnricher : IEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates a new instance of the user enricher.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public UserEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public int Order => 200;

    /// <inheritdoc/>
    public void Enrich(EventEnrichmentContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var sentryUser = new PluginSentryUser
        {
            Username = user.Identity.Name
        };

        var userIdClaim = user.FindFirst("sub")
            ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            ?? user.FindFirst("id");

        if (userIdClaim is not null)
        {
            sentryUser.Id = userIdClaim.Value;
        }

        var emailClaim = user.FindFirst("email")
            ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        if (emailClaim is not null)
        {
            sentryUser.Email = emailClaim.Value;
        }

        context.User = sentryUser;
    }
}
#endif
