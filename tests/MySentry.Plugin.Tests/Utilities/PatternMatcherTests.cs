using FluentAssertions;
using MySentry.Plugin.Utilities;
using Xunit;

namespace MySentry.Plugin.Tests.Utilities;

/// <summary>
/// Tests for PatternMatcher utility.
/// </summary>
public class PatternMatcherTests
{
    #region Matches (Single Pattern) Tests

    [Fact]
    public void Matches_ExactMatch_ReturnsTrue()
    {
        // Act
        var result = PatternMatcher.Matches("/api/users", "/api/users");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_ExactMatch_CaseInsensitive()
    {
        // Act
        var result = PatternMatcher.Matches("/API/Users", "/api/users");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Matches_NoMatch_ReturnsFalse()
    {
        // Act
        var result = PatternMatcher.Matches("/api/orders", "/api/users");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("/api/users", "/api/*", true)]
    [InlineData("/api/products", "/api/*", true)]
    [InlineData("/api/v1/users", "/api/*", true)]
    [InlineData("/health", "/api/*", false)]
    [InlineData("/other/api", "/api/*", false)]
    public void Matches_PrefixWildcard_Works(string value, string pattern, bool expected)
    {
        // Act
        var result = PatternMatcher.Matches(value, pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("data.json", "*.json", true)]
    [InlineData("config.json", "*.json", true)]
    [InlineData("data.xml", "*.json", false)]
    [InlineData("json", "*.json", false)]
    public void Matches_SuffixWildcard_Works(string value, string pattern, bool expected)
    {
        // Act
        var result = PatternMatcher.Matches(value, pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("/api/v1/users/list", "*users*", true)]
    [InlineData("/path/to/users/data", "*users*", true)]
    [InlineData("/api/products", "*users*", false)]
    public void Matches_ContainsWildcard_Works(string value, string pattern, bool expected)
    {
        // Act
        var result = PatternMatcher.Matches(value, pattern);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Matches_NullValue_ReturnsFalse()
    {
        // Act
        var result = PatternMatcher.Matches(null, "/api/*");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Matches_EmptyValue_ReturnsFalse()
    {
        // Act
        var result = PatternMatcher.Matches(string.Empty, "/api/*");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Matches_EmptyPattern_ReturnsFalse()
    {
        // Act
        var result = PatternMatcher.Matches("/api/users", string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region MatchesAny (Multiple Patterns) Tests

    [Fact]
    public void MatchesAny_FirstPatternMatches_ReturnsTrue()
    {
        // Arrange
        var patterns = new[] { "/health", "/ready", "/api/*" };

        // Act
        var result = PatternMatcher.MatchesAny("/health", patterns);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesAny_LastPatternMatches_ReturnsTrue()
    {
        // Arrange
        var patterns = new[] { "/health", "/ready", "/api/*" };

        // Act
        var result = PatternMatcher.MatchesAny("/api/users", patterns);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesAny_NoPatternMatches_ReturnsFalse()
    {
        // Arrange
        var patterns = new[] { "/health", "/ready", "/metrics" };

        // Act
        var result = PatternMatcher.MatchesAny("/api/users", patterns);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesAny_EmptyPatterns_ReturnsFalse()
    {
        // Arrange
        var patterns = Array.Empty<string>();

        // Act
        var result = PatternMatcher.MatchesAny("/api/users", patterns);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesAny_NullPatterns_ThrowsArgumentNullException()
    {
        // Act
        var act = () => PatternMatcher.MatchesAny("/api/users", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MatchesAny_NullValue_ReturnsFalse()
    {
        // Arrange
        var patterns = new[] { "/api/*", "/health" };

        // Act
        var result = PatternMatcher.MatchesAny(null, patterns);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesAny_MultipleWildcardPatterns_Works()
    {
        // Arrange
        var patterns = new[] { "/health*", "/ready*", "*.json", "*swagger*" };

        // Act & Assert
        PatternMatcher.MatchesAny("/health", patterns).Should().BeTrue();
        PatternMatcher.MatchesAny("/healthcheck", patterns).Should().BeTrue();
        PatternMatcher.MatchesAny("/ready", patterns).Should().BeTrue();
        PatternMatcher.MatchesAny("openapi.json", patterns).Should().BeTrue();
        PatternMatcher.MatchesAny("/swagger/index.html", patterns).Should().BeTrue();
        PatternMatcher.MatchesAny("/api/users", patterns).Should().BeFalse();
    }

    #endregion
}
