using Microsoft.AspNetCore.Mvc;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Features.Crons;
using MySentry.Plugin.Features.Tracing;
using MySentry.Plugin.Features.Profiling;
using MySentry.Plugin.Features.UserFeedback;

namespace MySentry.TestApp.Controllers;

/// <summary>
/// Controller demonstrating all MySentry.Plugin capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ISentryPlugin _sentry;
    private readonly ICronMonitor _cronMonitor;
    private readonly IUserFeedbackCapture _feedbackCapture;
    private readonly ILogger<TestController> _logger;

    public TestController(
        ISentryPlugin sentry,
        ICronMonitor cronMonitor,
        IUserFeedbackCapture feedbackCapture,
        ILogger<TestController> logger)
    {
        _sentry = sentry;
        _cronMonitor = cronMonitor;
        _feedbackCapture = feedbackCapture;
        _logger = logger;
    }

    // ========================================================================
    // ERROR CAPTURE DEMOS
    // ========================================================================

    /// <summary>
    /// Captures a test exception.
    /// </summary>
    [HttpGet("error/exception")]
    public IActionResult CaptureException()
    {
        try
        {
            throw new InvalidOperationException("This is a test exception from MySentry.TestApp");
        }
        catch (Exception ex)
        {
            var eventId = _sentry.CaptureException(ex);
            return Ok(new { Message = "Exception captured", EventId = eventId.ToString() });
        }
    }

    /// <summary>
    /// Captures a test message.
    /// </summary>
    [HttpGet("error/message")]
    public IActionResult CaptureMessage([FromQuery] string message = "Test message from MySentry.TestApp")
    {
        var eventId = _sentry.CaptureMessage(message);
        return Ok(new { Message = "Message captured", EventId = eventId.ToString() });
    }

    /// <summary>
    /// Captures a message with severity level.
    /// </summary>
    [HttpGet("error/message/{level}")]
    public IActionResult CaptureMessageWithLevel(string level, [FromQuery] string message = "Test message")
    {
        var severityLevel = level.ToLowerInvariant() switch
        {
            "debug" => PluginSeverityLevel.Debug,
            "info" => PluginSeverityLevel.Info,
            "warning" => PluginSeverityLevel.Warning,
            "error" => PluginSeverityLevel.Error,
            "fatal" => PluginSeverityLevel.Fatal,
            _ => PluginSeverityLevel.Info
        };

        var eventId = _sentry.CaptureMessage(message, severityLevel);
        return Ok(new { Message = "Message captured", Level = level, EventId = eventId.ToString() });
    }

    /// <summary>
    /// Throws an unhandled exception (will be caught by middleware).
    /// </summary>
    [HttpGet("error/unhandled")]
    public IActionResult ThrowUnhandledException()
    {
        throw new ApplicationException("This is an unhandled exception - should be caught by MySentryMiddleware");
    }

    // ========================================================================
    // BREADCRUMB DEMOS
    // ========================================================================

    /// <summary>
    /// Demonstrates breadcrumb tracking.
    /// </summary>
    [HttpGet("breadcrumbs")]
    public IActionResult AddBreadcrumbs()
    {
        _sentry.AddBreadcrumb("User started operation", "user.action");
        _sentry.AddBreadcrumb("Processing data", "processing", "default", PluginBreadcrumbLevel.Info);
        _sentry.AddBreadcrumb("Database query executed", "database", "query",
            new Dictionary<string, string> { ["query"] = "SELECT * FROM Users" },
            PluginBreadcrumbLevel.Debug);
        _sentry.AddBreadcrumb("Operation completed", "user.action", "default", PluginBreadcrumbLevel.Info);

        // Now capture an error to see all breadcrumbs
        var eventId = _sentry.CaptureMessage("Breadcrumb test completed", PluginSeverityLevel.Info);

        return Ok(new { Message = "Breadcrumbs added and captured", EventId = eventId.ToString() });
    }

    // ========================================================================
    // USER CONTEXT DEMOS
    // ========================================================================

    /// <summary>
    /// Sets user context and captures an event.
    /// </summary>
    [HttpGet("user")]
    public IActionResult SetUserContext(
        [FromQuery] string id = "user-123",
        [FromQuery] string email = "test@example.com",
        [FromQuery] string username = "testuser")
    {
        _sentry.SetUser(new PluginSentryUser
        {
            Id = id,
            Email = email,
            Username = username
        });

        var eventId = _sentry.CaptureMessage("User context test", PluginSeverityLevel.Info);
        return Ok(new { Message = "User context set", EventId = eventId.ToString() });
    }

    // ========================================================================
    // SCOPE DEMOS
    // ========================================================================

    /// <summary>
    /// Demonstrates scope configuration.
    /// </summary>
    [HttpGet("scope")]
    public IActionResult ConfigureScope()
    {
        _sentry.ConfigureScope(scope =>
        {
            scope.SetTag("feature", "scope-demo");
            scope.SetTag("version", "1.0.0");
            scope.SetExtra("timestamp", DateTime.UtcNow);
            scope.SetExtra("requestId", Guid.NewGuid().ToString());
            scope.SetContext("CustomContext", new
            {
                CustomField = "CustomValue",
                Number = 42,
                Nested = new { Inner = "Value" }
            });
        });

        var eventId = _sentry.CaptureMessage("Scope configuration test", PluginSeverityLevel.Info);
        return Ok(new { Message = "Scope configured", EventId = eventId.ToString() });
    }

    /// <summary>
    /// Demonstrates push/pop scope isolation.
    /// </summary>
    [HttpGet("scope/isolated")]
    public IActionResult IsolatedScope()
    {
        _sentry.ConfigureScope(scope => scope.SetTag("global", "value"));

        using (_sentry.PushScope())
        {
            _sentry.ConfigureScope(scope => scope.SetTag("isolated", "inner-value"));
            _sentry.CaptureMessage("Inside isolated scope", PluginSeverityLevel.Info);
        }

        // "isolated" tag no longer present
        var eventId = _sentry.CaptureMessage("Outside isolated scope", PluginSeverityLevel.Info);
        return Ok(new { Message = "Isolated scope test completed", EventId = eventId.ToString() });
    }

    // ========================================================================
    // TRACING DEMOS
    // ========================================================================

    /// <summary>
    /// Demonstrates manual transaction creation.
    /// </summary>
    [HttpGet("tracing/transaction")]
    public async Task<IActionResult> CreateTransaction()
    {
        using var transaction = _sentry.StartTransaction("test-transaction", "http.server");

        using (var span1 = transaction.StartChild("db.query", "Fetching users"))
        {
            await Task.Delay(50); // Simulate DB query
            span1.Finish(PluginSpanStatus.Ok);
        }

        using (var span2 = transaction.StartChild("serialize", "Serializing response"))
        {
            await Task.Delay(20); // Simulate serialization
            span2.Finish(PluginSpanStatus.Ok);
        }

        transaction.Finish(PluginSpanStatus.Ok);

        return Ok(new { Message = "Transaction completed", TransactionName = "test-transaction" });
    }

    /// <summary>
    /// Demonstrates extension-based span creation.
    /// </summary>
    [HttpGet("tracing/spans")]
    public async Task<IActionResult> SpanExtensions()
    {
        var result = await _sentry.Performance.WithTransactionAsync(
            "span-demo-transaction",
            "http.server",
            async () =>
            {
                // Nested spans
                await _sentry.Performance.WithSpanAsync("db.query", "Query 1", async () =>
                {
                    await Task.Delay(30);
                });

                var data = await _sentry.Performance.WithSpanAsync("db.query", "Query 2", async () =>
                {
                    await Task.Delay(20);
                    return new { Result = "Data from query" };
                });

                return data;
            });

        return Ok(new { Message = "Span demo completed", Data = result });
    }

    // ========================================================================
    // PROFILING DEMOS
    // ========================================================================

    /// <summary>
    /// Demonstrates profiled operations.
    /// </summary>
    [HttpGet("profiling")]
    public async Task<IActionResult> ProfilingDemo()
    {
        // Profile a simple operation
        _sentry.Profiled("sync-operation", () =>
        {
            Thread.Sleep(50);
        });

        // Profile an async operation
        var result = await _sentry.ProfiledAsync("async-operation", async () =>
        {
            await Task.Delay(100);
            return "Async result";
        });

        // Profile with explicit control
        using (var profile = _sentry.Profile("custom-operation", "Custom profiled operation"))
        {
            await Task.Delay(75);
            profile.Success();
        }

        return Ok(new { Message = "Profiling demo completed", Result = result });
    }

    // ========================================================================
    // CRON MONITORING DEMOS
    // ========================================================================

    /// <summary>
    /// Demonstrates cron job monitoring.
    /// </summary>
    [HttpGet("crons/simple")]
    public async Task<IActionResult> SimpleCronMonitor()
    {
        await _cronMonitor.WithCronMonitoringAsync("test-cron-job", async () =>
        {
            _logger.LogInformation("Cron job started");
            await Task.Delay(100); // Simulate job work
            _logger.LogInformation("Cron job completed");
        });

        return Ok(new { Message = "Cron job completed successfully" });
    }

    /// <summary>
    /// Demonstrates cron job with manual control.
    /// </summary>
    [HttpGet("crons/manual")]
    public async Task<IActionResult> ManualCronMonitor([FromQuery] bool shouldFail = false)
    {
        using var monitor = _cronMonitor.MonitorCronJob("manual-cron-job");

        try
        {
            _logger.LogInformation("Manual cron job started");
            await Task.Delay(100);

            if (shouldFail)
            {
                throw new InvalidOperationException("Simulated cron job failure");
            }

            monitor.Complete();
            return Ok(new { Message = "Manual cron job completed successfully" });
        }
        catch (Exception ex)
        {
            monitor.Fail();
            _sentry.CaptureException(ex);
            return StatusCode(500, new { Message = "Cron job failed", Error = ex.Message });
        }
    }

    // ========================================================================
    // USER FEEDBACK DEMOS
    // ========================================================================

    /// <summary>
    /// Submits user feedback.
    /// </summary>
    [HttpPost("feedback")]
    public IActionResult SubmitFeedback([FromBody] FeedbackDto dto)
    {
        // First capture an event to associate feedback with
        var eventId = _sentry.CaptureMessage("User feedback received", PluginSeverityLevel.Info);

        // Attach the feedback
        _feedbackCapture.CaptureFeedback(eventId, dto.Name ?? "", dto.Email ?? "", dto.Comments);

        return Ok(new { Message = "Feedback submitted", EventId = eventId.ToString() });
    }

    /// <summary>
    /// Submits feedback for an exception.
    /// </summary>
    [HttpPost("feedback/exception")]
    public IActionResult FeedbackWithException([FromBody] FeedbackDto dto)
    {
        try
        {
            throw new InvalidOperationException("Simulated error for feedback demo");
        }
        catch (Exception ex)
        {
            var eventId = _sentry.CaptureException(ex);
            _feedbackCapture.CaptureFeedback(eventId, dto.Name ?? "", dto.Email ?? "", dto.Comments);

            return Ok(new { Message = "Exception captured with feedback", EventId = eventId.ToString() });
        }
    }

    // ========================================================================
    // COMBINED DEMO
    // ========================================================================

    /// <summary>
    /// Demonstrates multiple features working together.
    /// </summary>
    [HttpGet("demo/full")]
    public async Task<IActionResult> FullDemo()
    {
        // Set user context
        _sentry.SetUser(new PluginSentryUser
        {
            Id = "demo-user",
            Email = "demo@example.com",
            Username = "Demo User"
        });

        // Configure scope
        _sentry.ConfigureScope(scope =>
        {
            scope.SetTag("demo", "full");
            scope.SetTag("feature", "comprehensive-test");
        });

        // Add breadcrumbs
        _sentry.AddBreadcrumb("Full demo started", "demo");

        // Create a transaction with spans
        using var transaction = _sentry.StartTransaction("full-demo-transaction", "http.server");

        // Profiled operations within the transaction
        using (var span = transaction.StartChild("db.query", "Database operation"))
        {
            await _sentry.ProfiledAsync("db-query", async () =>
            {
                await Task.Delay(50);
            });
            span.Finish(PluginSpanStatus.Ok);
        }

        _sentry.AddBreadcrumb("Database operation completed", "demo");

        using (var span = transaction.StartChild("cache.get", "Cache lookup"))
        {
            await Task.Delay(20);
            span.Finish(PluginSpanStatus.Ok);
        }

        _sentry.AddBreadcrumb("Cache lookup completed", "demo");

        transaction.Finish(PluginSpanStatus.Ok);

        // Capture a summary message
        var eventId = _sentry.CaptureMessage("Full demo completed successfully", PluginSeverityLevel.Info);

        return Ok(new
        {
            Message = "Full demo completed",
            EventId = eventId.ToString(),
            Features = new[]
            {
                "User Context",
                "Scope Configuration",
                "Breadcrumbs",
                "Transactions",
                "Spans",
                "Profiling"
            }
        });
    }
}

/// <summary>
/// DTO for user feedback submissions.
/// </summary>
public record FeedbackDto
{
    public string? Email { get; init; }
    public string? Name { get; init; }
    public required string Comments { get; init; }
}
