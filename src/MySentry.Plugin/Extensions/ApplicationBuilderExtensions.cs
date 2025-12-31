#if ASPNETCORE
using Microsoft.AspNetCore.Builder;
using MySentry.Plugin.Middleware;

namespace MySentry.Plugin.Extensions;

/// <summary>
/// Extension methods for configuring MySentry middleware on <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds MySentry middleware to the application pipeline.
    /// This should be called early in the pipeline to capture all errors.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseMySentry(this IApplicationBuilder app)
    {
        app.UseMiddleware<MySentryMiddleware>();
        app.UseSentryTracing();

        return app;
    }

    /// <summary>
    /// Adds MySentry middleware to the application pipeline for WebApplication.
    /// This should be called early in the pipeline to capture all errors.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseMySentry(this WebApplication app)
    {
        app.UseMiddleware<MySentryMiddleware>();
        app.UseSentryTracing();

        return app;
    }
}
#endif
