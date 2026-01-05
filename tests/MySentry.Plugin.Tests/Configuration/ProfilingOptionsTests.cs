using FluentAssertions;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for ProfilingOptions.
/// </summary>
public class ProfilingOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ProfilingOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.SampleRate.Should().Be(SamplingRates.Disabled);
        options.StartupTimeout.Should().BeNull();
    }

    [Fact]
    public void Enabled_CanBeSet()
    {
        // Arrange
        var options = new ProfilingOptions();

        // Act
        options.Enabled = true;

        // Assert
        options.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void SampleRate_CanBeSet(double rate)
    {
        // Arrange
        var options = new ProfilingOptions();

        // Act
        options.SampleRate = rate;

        // Assert
        options.SampleRate.Should().Be(rate);
    }

    [Fact]
    public void StartupTimeout_CanBeSet()
    {
        // Arrange
        var options = new ProfilingOptions();
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        options.StartupTimeout = timeout;

        // Assert
        options.StartupTimeout.Should().Be(timeout);
    }

    [Fact]
    public void StartupTimeout_CanBeSetToNull()
    {
        // Arrange
        var options = new ProfilingOptions
        {
            StartupTimeout = TimeSpan.FromSeconds(10)
        };

        // Act
        options.StartupTimeout = null;

        // Assert
        options.StartupTimeout.Should().BeNull();
    }
}
