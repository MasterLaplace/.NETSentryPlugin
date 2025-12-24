using FluentAssertions;
using Microsoft.Extensions.Logging;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for <see cref="SentryPluginOptions"/>.
/// </summary>
public class SentryPluginOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new SentryPluginOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.Debug.Should().BeFalse();
        options.DiagnosticLevel.Should().Be(DiagnosticLevel.Warning);
        options.SampleRate.Should().Be(1.0);
        options.MaxBreadcrumbs.Should().Be(100);
        options.AttachStacktrace.Should().BeTrue();
        options.SendDefaultPii.Should().BeFalse();
        options.MinimumBreadcrumbLevel.Should().Be(LogLevel.Information);
        options.MinimumEventLevel.Should().Be(LogLevel.Error);
        options.ShutdownTimeout.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Dsn_CanBeSet()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.Dsn = "https://test@sentry.io/123";

        // Assert
        options.Dsn.Should().Be("https://test@sentry.io/123");
    }

    [Fact]
    public void Environment_CanBeSet()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.Environment = "production";

        // Assert
        options.Environment.Should().Be("production");
    }

    [Fact]
    public void Release_CanBeSet()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.Release = "1.0.0";

        // Assert
        options.Release.Should().Be("1.0.0");
    }

    [Fact]
    public void TracingOptions_HasCorrectDefaults()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Assert
        options.Tracing.Should().NotBeNull();
        options.Tracing.Enabled.Should().BeFalse();
        options.Tracing.SampleRate.Should().Be(SamplingRates.Disabled);
        options.Tracing.TraceAllRequests.Should().BeTrue();
    }

    [Fact]
    public void ProfilingOptions_HasCorrectDefaults()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Assert
        options.Profiling.Should().NotBeNull();
        options.Profiling.Enabled.Should().BeFalse();
        options.Profiling.SampleRate.Should().Be(SamplingRates.Disabled);
    }

    [Fact]
    public void FilteringOptions_HasCorrectDefaults()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Assert
        options.Filtering.Should().NotBeNull();
        options.Filtering.IgnoreExceptionTypes.Should().NotBeNull();
        options.Filtering.IgnoreStatusCodes.Should().NotBeNull();
        options.Filtering.IgnoreUrls.Should().NotBeNull();
    }

    [Fact]
    public void SampleRate_ValidValue_Works()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.SampleRate = 0.5;

        // Assert
        options.SampleRate.Should().Be(0.5);
    }

    [Fact]
    public void MaxBreadcrumbs_CanBeModified()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.MaxBreadcrumbs = 50;

        // Assert
        options.MaxBreadcrumbs.Should().Be(50);
    }

    [Fact]
    public void ServerName_CanBeSet()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.ServerName = "server-01";

        // Assert
        options.ServerName.Should().Be("server-01");
    }

    [Fact]
    public void DiagnosticLevel_CanBeSet()
    {
        // Arrange
        var options = new SentryPluginOptions();

        // Act
        options.DiagnosticLevel = DiagnosticLevel.Debug;

        // Assert
        options.DiagnosticLevel.Should().Be(DiagnosticLevel.Debug);
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        // Assert
        SentryPluginOptions.SectionName.Should().Be("MySentry");
    }
}
