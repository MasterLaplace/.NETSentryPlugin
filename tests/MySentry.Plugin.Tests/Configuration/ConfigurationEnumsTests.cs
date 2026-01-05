using FluentAssertions;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for configuration enums.
/// </summary>
public class ConfigurationEnumsTests
{
    #region DiagnosticLevel Tests

    [Fact]
    public void DiagnosticLevel_HasCorrectValues()
    {
        // Assert
        ((int)DiagnosticLevel.Debug).Should().Be(0);
        ((int)DiagnosticLevel.Info).Should().Be(1);
        ((int)DiagnosticLevel.Warning).Should().Be(2);
        ((int)DiagnosticLevel.Error).Should().Be(3);
        ((int)DiagnosticLevel.Fatal).Should().Be(4);
    }

    [Fact]
    public void DiagnosticLevel_OrderIsCorrect()
    {
        // Arrange
        var levels = new[]
        {
            DiagnosticLevel.Debug,
            DiagnosticLevel.Info,
            DiagnosticLevel.Warning,
            DiagnosticLevel.Error,
            DiagnosticLevel.Fatal
        };

        // Assert - Verify they're in ascending order (compare as int)
        for (int i = 0; i < levels.Length - 1; i++)
        {
            ((int)levels[i]).Should().BeLessThan((int)levels[i + 1]);
        }
    }

    [Theory]
    [InlineData(DiagnosticLevel.Debug)]
    [InlineData(DiagnosticLevel.Info)]
    [InlineData(DiagnosticLevel.Warning)]
    [InlineData(DiagnosticLevel.Error)]
    [InlineData(DiagnosticLevel.Fatal)]
    public void DiagnosticLevel_IsDefined(DiagnosticLevel level)
    {
        // Assert
        Enum.IsDefined(typeof(DiagnosticLevel), level).Should().BeTrue();
    }

    #endregion

    #region RequestBodySize Tests

    [Fact]
    public void RequestBodySize_HasCorrectValues()
    {
        // Assert
        ((int)RequestBodySize.None).Should().Be(0);
        ((int)RequestBodySize.Small).Should().Be(1);
        ((int)RequestBodySize.Medium).Should().Be(2);
        ((int)RequestBodySize.Always).Should().Be(3);
    }

    [Fact]
    public void RequestBodySize_OrderIsCorrect()
    {
        // Arrange
        var sizes = new[]
        {
            RequestBodySize.None,
            RequestBodySize.Small,
            RequestBodySize.Medium,
            RequestBodySize.Always
        };

        // Assert - Verify they're in ascending order (compare as int)
        for (int i = 0; i < sizes.Length - 1; i++)
        {
            ((int)sizes[i]).Should().BeLessThan((int)sizes[i + 1]);
        }
    }

    [Theory]
    [InlineData(RequestBodySize.None)]
    [InlineData(RequestBodySize.Small)]
    [InlineData(RequestBodySize.Medium)]
    [InlineData(RequestBodySize.Always)]
    public void RequestBodySize_IsDefined(RequestBodySize size)
    {
        // Assert
        Enum.IsDefined(typeof(RequestBodySize), size).Should().BeTrue();
    }

    [Fact]
    public void RequestBodySize_None_IsDefault()
    {
        // Act
        var defaultSize = default(RequestBodySize);

        // Assert
        defaultSize.Should().Be(RequestBodySize.None);
    }

    #endregion
}
