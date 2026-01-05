using FluentAssertions;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for SamplingRates constants.
/// </summary>
public class SamplingRatesTests
{
    [Fact]
    public void Disabled_IsZero()
    {
        // Assert
        SamplingRates.Disabled.Should().Be(0.0);
    }

    [Fact]
    public void Minimal_IsOnePercent()
    {
        // Assert
        SamplingRates.Minimal.Should().Be(0.01);
    }

    [Fact]
    public void Low_IsTenPercent()
    {
        // Assert
        SamplingRates.Low.Should().Be(0.1);
    }

    [Fact]
    public void Standard_IsTwentyFivePercent()
    {
        // Assert
        SamplingRates.Standard.Should().Be(0.25);
    }

    [Fact]
    public void RecommendedProduction_IsFiftyPercent()
    {
        // Assert
        SamplingRates.RecommendedProduction.Should().Be(0.5);
    }

    [Fact]
    public void High_IsSeventyFivePercent()
    {
        // Assert
        SamplingRates.High.Should().Be(0.75);
    }

    [Fact]
    public void All_IsOneHundredPercent()
    {
        // Assert
        SamplingRates.All.Should().Be(1.0);
    }

    [Fact]
    public void RatesAreInAscendingOrder()
    {
        // Assert
        SamplingRates.Disabled.Should().BeLessThan(SamplingRates.Minimal);
        SamplingRates.Minimal.Should().BeLessThan(SamplingRates.Low);
        SamplingRates.Low.Should().BeLessThan(SamplingRates.Standard);
        SamplingRates.Standard.Should().BeLessThan(SamplingRates.RecommendedProduction);
        SamplingRates.RecommendedProduction.Should().BeLessThan(SamplingRates.High);
        SamplingRates.High.Should().BeLessThan(SamplingRates.All);
    }

    [Fact]
    public void AllRatesAreWithinValidRange()
    {
        // Arrange
        var rates = new[]
        {
            SamplingRates.Disabled,
            SamplingRates.Minimal,
            SamplingRates.Low,
            SamplingRates.Standard,
            SamplingRates.RecommendedProduction,
            SamplingRates.High,
            SamplingRates.All
        };

        // Assert
        foreach (var rate in rates)
        {
            rate.Should().BeGreaterOrEqualTo(0.0);
            rate.Should().BeLessOrEqualTo(1.0);
        }
    }
}
