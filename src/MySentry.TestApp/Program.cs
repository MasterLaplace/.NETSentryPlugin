using MySentry.Plugin.Configuration;
using MySentry.Plugin.Extensions;
using MySentry.Plugin.Features.UserFeedback;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// OPTION 1: Configuration via appsettings.json (recommended for production)
// ============================================================================
// builder.AddMySentry();

// ============================================================================
// OPTION 2: Fluent configuration (full control)
// ============================================================================
builder.AddMySentry(config =>
{
    config.WithDsn(builder.Configuration["MySentry:Dsn"] ?? string.Empty)
        .WithEnvironment(builder.Environment.EnvironmentName)
        .WithRelease("MySentry.TestApp@1.0.0")
        .WithMaxBreadcrumbs(100)
        .WithStackTrace(true);

    // Enable debug in development
    if (builder.Environment.IsDevelopment())
    {
        config.WithDebug(true, DiagnosticLevel.Debug);
    }

    // Configure tracing
    config.EnableTracing(tracing => tracing.WithSampleRate(builder.Environment.IsDevelopment() ? 1.0 : 0.2)
            .TraceAllRequests(true)
            .IgnoreUrls("/health*", "/swagger*"));

    // Configure profiling
    config.EnableProfiling(profiling => profiling.WithSampleRate(builder.Environment.IsDevelopment() ? 1.0 : 0.1));

    // Configure filtering
    config.FilterEvents(filtering => filtering.IgnoreExceptionTypes("System.OperationCanceledException")
            .IgnoreStatusCodes(404));
});

// Add user feedback services
builder.Services.AddMySentryFeedback();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "MySentry TestApp API", Version = "v1" }));

builder.Services.AddControllers();

var app = builder.Build();

// Enable Sentry middleware uniquement si DSN présent
var dsn = builder.Configuration["MySentry:Dsn"];
if (!string.IsNullOrWhiteSpace(dsn))
{
    app.UseMySentry();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();
