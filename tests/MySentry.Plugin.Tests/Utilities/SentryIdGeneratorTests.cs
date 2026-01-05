using FluentAssertions;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Utilities;
using Xunit;

using PluginSentryEventId = MySentry.Plugin.Abstractions.SentryEventId;

namespace MySentry.Plugin.Tests.Utilities;

/// <summary>
/// Tests for SentryIdGenerator.
/// </summary>
public class SentryIdGeneratorTests
{
    #region Generate Tests

    [Fact]
    public void Generate_ReturnsNonEmptyId()
    {
        // Act
        var id = SentryIdGenerator.Generate();

        // Assert
        id.Should().NotBe(PluginSentryEventId.Empty);
    }

    [Fact]
    public void Generate_ReturnsUniqueIds()
    {
        // Act
        var id1 = SentryIdGenerator.Generate();
        var id2 = SentryIdGenerator.Generate();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Generate_MultipleIds_AllUnique()
    {
        // Arrange
        var ids = new HashSet<PluginSentryEventId>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            ids.Add(SentryIdGenerator.Generate());
        }

        // Assert - All 100 IDs should be unique
        ids.Should().HaveCount(100);
    }

    #endregion

    #region Parse Tests

    [Fact]
    public void Parse_ValidGuid_ReturnsEventId()
    {
        // Arrange
        var guidString = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        // Act
        var id = SentryIdGenerator.Parse(guidString);

        // Assert
        id.ToString().Should().Be("a1b2c3d4e5f67890abcdef1234567890");
    }

    [Fact]
    public void Parse_ValidGuidWithoutHyphens_ReturnsEventId()
    {
        // Arrange
        var guidString = "a1b2c3d4e5f67890abcdef1234567890";

        // Act
        var id = SentryIdGenerator.Parse(guidString);

        // Assert
        id.Should().NotBe(PluginSentryEventId.Empty);
    }

    [Fact]
    public void Parse_InvalidString_ThrowsFormatException()
    {
        // Arrange
        var invalidString = "not-a-guid";

        // Act
        var act = () => SentryIdGenerator.Parse(invalidString);

        // Assert
        act.Should().Throw<FormatException>()
           .WithMessage("*is not a valid Sentry event ID*");
    }

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        // Arrange
        var emptyString = "";

        // Act
        var act = () => SentryIdGenerator.Parse(emptyString);

        // Assert
        act.Should().Throw<FormatException>();
    }

    #endregion

    #region TryParse Tests

    [Fact]
    public void TryParse_ValidGuid_ReturnsTrueAndEventId()
    {
        // Arrange
        var guidString = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        // Act
        var result = SentryIdGenerator.TryParse(guidString, out var id);

        // Assert
        result.Should().BeTrue();
        id.Should().NotBe(PluginSentryEventId.Empty);
    }

    [Fact]
    public void TryParse_InvalidString_ReturnsFalseAndEmptyId()
    {
        // Arrange
        var invalidString = "not-a-guid";

        // Act
        var result = SentryIdGenerator.TryParse(invalidString, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().Be(PluginSentryEventId.Empty);
    }

    [Fact]
    public void TryParse_NullString_ReturnsFalseAndEmptyId()
    {
        // Arrange
        string? nullString = null;

        // Act
        var result = SentryIdGenerator.TryParse(nullString, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().Be(PluginSentryEventId.Empty);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalseAndEmptyId()
    {
        // Arrange
        var emptyString = "";

        // Act
        var result = SentryIdGenerator.TryParse(emptyString, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().Be(PluginSentryEventId.Empty);
    }

    [Fact]
    public void TryParse_WhitespaceString_ReturnsFalseAndEmptyId()
    {
        // Arrange
        var whitespace = "   ";

        // Act
        var result = SentryIdGenerator.TryParse(whitespace, out var id);

        // Assert
        result.Should().BeFalse();
        id.Should().Be(PluginSentryEventId.Empty);
    }

    #endregion

    #region Roundtrip Tests

    [Fact]
    public void GenerateParseTryParse_Roundtrip()
    {
        // Arrange
        var original = SentryIdGenerator.Generate();
        var stringRep = original.ToString();

        // Act
        var parsed = SentryIdGenerator.Parse(stringRep);
        var tryParsed = SentryIdGenerator.TryParse(stringRep, out var result);

        // Assert
        parsed.Should().Be(original);
        tryParsed.Should().BeTrue();
        result.Should().Be(original);
    }

    #endregion
}
