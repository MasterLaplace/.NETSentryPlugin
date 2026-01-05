using FluentAssertions;
using MySentry.Plugin.Abstractions;
using Xunit;

using PluginSeverityLevel = MySentry.Plugin.Abstractions.SeverityLevel;
using PluginBreadcrumbLevel = MySentry.Plugin.Abstractions.BreadcrumbLevel;
using PluginSpanStatus = MySentry.Plugin.Abstractions.SpanStatus;

namespace MySentry.Plugin.Tests.Abstractions;

/// <summary>
/// Tests for SeverityLevel, BreadcrumbLevel, and SpanStatus enums.
/// </summary>
public class EnumTests
{
    #region SeverityLevel Tests

    [Fact]
    public void SeverityLevel_HasCorrectValues()
    {
        // Assert
        ((int)PluginSeverityLevel.Debug).Should().Be(0);
        ((int)PluginSeverityLevel.Info).Should().Be(1);
        ((int)PluginSeverityLevel.Warning).Should().Be(2);
        ((int)PluginSeverityLevel.Error).Should().Be(3);
        ((int)PluginSeverityLevel.Fatal).Should().Be(4);
    }

    [Fact]
    public void SeverityLevel_Debug_IsLessThanInfo()
    {
        // Assert - Compare as int values
        ((int)PluginSeverityLevel.Debug).Should().BeLessThan((int)PluginSeverityLevel.Info);
    }

    [Fact]
    public void SeverityLevel_Error_IsLessThanFatal()
    {
        // Assert - Compare as int values
        ((int)PluginSeverityLevel.Error).Should().BeLessThan((int)PluginSeverityLevel.Fatal);
    }

    [Fact]
    public void SeverityLevel_OrderIsCorrect()
    {
        // Arrange
        var levels = new[] 
        { 
            PluginSeverityLevel.Debug, 
            PluginSeverityLevel.Info, 
            PluginSeverityLevel.Warning, 
            PluginSeverityLevel.Error, 
            PluginSeverityLevel.Fatal 
        };

        // Assert - Verify they're in ascending order
        for (int i = 0; i < levels.Length - 1; i++)
        {
            ((int)levels[i]).Should().BeLessThan((int)levels[i + 1]);
        }
    }

    #endregion

    #region BreadcrumbLevel Tests

    [Fact]
    public void BreadcrumbLevel_HasCorrectValues()
    {
        // Assert
        ((int)PluginBreadcrumbLevel.Debug).Should().Be(0);
        ((int)PluginBreadcrumbLevel.Info).Should().Be(1);
        ((int)PluginBreadcrumbLevel.Warning).Should().Be(2);
        ((int)PluginBreadcrumbLevel.Error).Should().Be(3);
        ((int)PluginBreadcrumbLevel.Fatal).Should().Be(4);
    }

    [Fact]
    public void BreadcrumbLevel_Debug_IsLessThanInfo()
    {
        // Assert - Compare as int values
        ((int)PluginBreadcrumbLevel.Debug).Should().BeLessThan((int)PluginBreadcrumbLevel.Info);
    }

    [Fact]
    public void BreadcrumbLevel_Error_IsLessThanFatal()
    {
        // Assert - Compare as int values
        ((int)PluginBreadcrumbLevel.Error).Should().BeLessThan((int)PluginBreadcrumbLevel.Fatal);
    }

    [Fact]
    public void BreadcrumbLevel_OrderIsCorrect()
    {
        // Arrange
        var levels = new[] 
        { 
            PluginBreadcrumbLevel.Debug, 
            PluginBreadcrumbLevel.Info, 
            PluginBreadcrumbLevel.Warning, 
            PluginBreadcrumbLevel.Error, 
            PluginBreadcrumbLevel.Fatal 
        };

        // Assert - Verify they're in ascending order
        for (int i = 0; i < levels.Length - 1; i++)
        {
            ((int)levels[i]).Should().BeLessThan((int)levels[i + 1]);
        }
    }

    #pragma warning disable CS0618 // Testing obsolete member intentionally
    [Fact]
    public void BreadcrumbLevel_Critical_EqualsFatal()
    {
        // Assert - Critical is an alias for Fatal
        PluginBreadcrumbLevel.Critical.Should().Be(PluginBreadcrumbLevel.Fatal);
    }
    #pragma warning restore CS0618

    #endregion

    #region SpanStatus Tests

    [Fact]
    public void SpanStatus_HasExpectedMembers()
    {
        // Assert - Check key members exist
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.Ok).Should().BeTrue();
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.Cancelled).Should().BeTrue();
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.Unknown).Should().BeTrue();
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.InvalidArgument).Should().BeTrue();
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.DeadlineExceeded).Should().BeTrue();
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.NotFound).Should().BeTrue();
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.PermissionDenied).Should().BeTrue();
    }

    [Fact]
    public void SpanStatus_Ok_HasValueZero()
    {
        // Assert
        ((int)PluginSpanStatus.Ok).Should().Be(0);
    }

    [Fact]
    public void SpanStatus_Cancelled_HasValueOne()
    {
        // Assert
        ((int)PluginSpanStatus.Cancelled).Should().Be(1);
    }

    [Fact]
    public void SpanStatus_HasInternalError()
    {
        // Assert
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.InternalError).Should().BeTrue();
    }

    [Fact]
    public void SpanStatus_HasUnimplemented()
    {
        // Assert
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.Unimplemented).Should().BeTrue();
    }

    [Fact]
    public void SpanStatus_HasUnavailable()
    {
        // Assert
        Enum.IsDefined(typeof(PluginSpanStatus), PluginSpanStatus.Unavailable).Should().BeTrue();
    }

    #endregion

    #region Conversion Tests

    [Theory]
    [InlineData(0, PluginSeverityLevel.Debug)]
    [InlineData(1, PluginSeverityLevel.Info)]
    [InlineData(2, PluginSeverityLevel.Warning)]
    [InlineData(3, PluginSeverityLevel.Error)]
    [InlineData(4, PluginSeverityLevel.Fatal)]
    public void SeverityLevel_CanBeCastFromInt(int value, PluginSeverityLevel expected)
    {
        // Act
        var level = (PluginSeverityLevel)value;

        // Assert
        level.Should().Be(expected);
    }

    [Theory]
    [InlineData(PluginSeverityLevel.Debug, 0)]
    [InlineData(PluginSeverityLevel.Info, 1)]
    [InlineData(PluginSeverityLevel.Warning, 2)]
    [InlineData(PluginSeverityLevel.Error, 3)]
    [InlineData(PluginSeverityLevel.Fatal, 4)]
    public void SeverityLevel_CanBeCastToInt(PluginSeverityLevel level, int expected)
    {
        // Act
        var value = (int)level;

        // Assert
        value.Should().Be(expected);
    }

    #endregion
}
