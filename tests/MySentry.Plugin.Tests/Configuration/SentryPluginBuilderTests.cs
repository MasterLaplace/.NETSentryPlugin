using FluentAssertions;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for <see cref="SentryPluginBuilder"/>.
/// </summary>
public class SentryPluginBuilderTests
{
    [Fact]
    public void WithDsn_SetsCorrectValue()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.WithDsn("https://test@sentry.io/123");

        // Assert
        builder.Options.Dsn.Should().Be("https://test@sentry.io/123");
    }

    [Fact]
    public void WithEnvironment_SetsCorrectValue()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.WithEnvironment("production");

        // Assert
        builder.Options.Environment.Should().Be("production");
    }

    [Fact]
    public void WithRelease_SetsCorrectValue()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.WithRelease("1.0.0");

        // Assert
        builder.Options.Release.Should().Be("1.0.0");
    }

    [Fact]
    public void WithDebug_EnablesDebugMode()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.WithDebug(true, DiagnosticLevel.Debug);

        // Assert
        builder.Options.Debug.Should().BeTrue();
        builder.Options.DiagnosticLevel.Should().Be(DiagnosticLevel.Debug);
    }

    [Fact]
    public void EnableTracing_ConfiguresTracingOptions()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.EnableTracing(tracing =>
        {
            tracing.WithSampleRate(0.5)
                .TraceAllRequests(true);
        });

        // Assert
        builder.Options.Tracing.Enabled.Should().BeTrue();
        builder.Options.Tracing.SampleRate.Should().Be(0.5);
        builder.Options.Tracing.TraceAllRequests.Should().BeTrue();
    }

    [Fact]
    public void EnableProfiling_ConfiguresProfilingOptions()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.EnableProfiling(profiling =>
        {
            profiling.WithSampleRate(0.2);
        });

        // Assert
        builder.Options.Profiling.Enabled.Should().BeTrue();
        builder.Options.Profiling.SampleRate.Should().Be(0.2);
    }

    [Fact]
    public void FilterEvents_ConfiguresFiltering()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.FilterEvents(filtering =>
        {
            filtering.IgnoreExceptionTypes("System.ArgumentException", "System.InvalidOperationException")
                .IgnoreStatusCodes(404, 401);
        });

        // Assert
        builder.Options.Filtering.IgnoreExceptionTypes
            .Should().Contain("System.ArgumentException")
            .And.Contain("System.InvalidOperationException");
        builder.Options.Filtering.IgnoreStatusCodes
            .Should().Contain(404)
            .And.Contain(401);
    }

    [Fact]
    public void FluentChaining_WorksCorrectly()
    {
        // Arrange & Act
        var builder = new SentryPluginBuilder()
            .WithDsn("https://test@sentry.io/123")
            .WithEnvironment("staging")
            .WithRelease("2.0.0")
            .WithDebug(true)
            .WithMaxBreadcrumbs(50)
            .WithStackTrace(true)
            .WithDefaultPii(true);

        // Assert
        builder.Options.Dsn.Should().Be("https://test@sentry.io/123");
        builder.Options.Environment.Should().Be("staging");
        builder.Options.Release.Should().Be("2.0.0");
        builder.Options.Debug.Should().BeTrue();
        builder.Options.MaxBreadcrumbs.Should().Be(50);
        builder.Options.AttachStacktrace.Should().BeTrue();
        builder.Options.SendDefaultPii.Should().BeTrue();
    }

    [Fact]
    public void WithServerName_SetsCorrectValue()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.WithServerName("server-01");

        // Assert
        builder.Options.ServerName.Should().Be("server-01");
    }

    [Fact]
    public void WithSampleRate_SetsCorrectValue()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.WithSampleRate(0.75);

        // Assert
        builder.Options.SampleRate.Should().Be(0.75);
    }

    [Fact]
    public void Tracing_IgnoreUrls_AddsPatterns()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Act
        builder.EnableTracing(tracing =>
        {
            tracing.IgnoreUrls("/health*", "/swagger*");
        });

        // Assert
        builder.Options.Tracing.IgnoreUrls
            .Should().Contain("/health*")
            .And.Contain("/swagger*");
    }

    [Fact]
    public void DefaultOptions_HaveCorrectValues()
    {
        // Arrange
        var builder = new SentryPluginBuilder();

        // Assert
        builder.Options.Enabled.Should().BeTrue();
        builder.Options.Debug.Should().BeFalse();
        builder.Options.MaxBreadcrumbs.Should().Be(100);
        builder.Options.SampleRate.Should().Be(1.0);
        builder.Options.AttachStacktrace.Should().BeTrue();
    }
}
