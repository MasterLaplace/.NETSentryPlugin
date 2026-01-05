using FluentAssertions;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for TracingOptions.
/// </summary>
public class TracingOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new TracingOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.SampleRate.Should().Be(SamplingRates.Disabled);
        options.TraceAllRequests.Should().BeTrue();
        options.TraceDatabase.Should().BeTrue();
        options.TraceHttpClients.Should().BeTrue();
        options.PropagateTraceparent.Should().BeFalse();
    }

    [Fact]
    public void IgnoreUrls_HasDefaultEntries()
    {
        // Arrange & Act
        var options = new TracingOptions();

        // Assert
        options.IgnoreUrls.Should().NotBeEmpty();
        options.IgnoreUrls.Should().Contain("/health");
        options.IgnoreUrls.Should().Contain("/healthz");
        options.IgnoreUrls.Should().Contain("/metrics");
        options.IgnoreUrls.Should().Contain("/ready");
        options.IgnoreUrls.Should().Contain("/favicon.ico");
    }

    [Fact]
    public void IgnoreTransactions_IsEmptyByDefault()
    {
        // Arrange & Act
        var options = new TracingOptions();

        // Assert
        options.IgnoreTransactions.Should().NotBeNull();
        options.IgnoreTransactions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void SampleRate_CanBeSet(double rate)
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.SampleRate = rate;

        // Assert
        options.SampleRate.Should().Be(rate);
    }

    [Fact]
    public void Enabled_CanBeSet()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.Enabled = true;

        // Assert
        options.Enabled.Should().BeTrue();
    }

    [Fact]
    public void PropagateTraceparent_CanBeSet()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.PropagateTraceparent = true;

        // Assert
        options.PropagateTraceparent.Should().BeTrue();
    }

    [Fact]
    public void TraceAllRequests_CanBeDisabled()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.TraceAllRequests = false;

        // Assert
        options.TraceAllRequests.Should().BeFalse();
    }

    [Fact]
    public void TraceDatabase_CanBeDisabled()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.TraceDatabase = false;

        // Assert
        options.TraceDatabase.Should().BeFalse();
    }

    [Fact]
    public void TraceHttpClients_CanBeDisabled()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.TraceHttpClients = false;

        // Assert
        options.TraceHttpClients.Should().BeFalse();
    }

    [Fact]
    public void IgnoreUrls_CanBeModified()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.IgnoreUrls.Add("/custom/path");

        // Assert
        options.IgnoreUrls.Should().Contain("/custom/path");
    }

    [Fact]
    public void IgnoreTransactions_CanBeModified()
    {
        // Arrange
        var options = new TracingOptions();

        // Act
        options.IgnoreTransactions.Add("IgnoredTransaction");

        // Assert
        options.IgnoreTransactions.Should().Contain("IgnoredTransaction");
    }
}
