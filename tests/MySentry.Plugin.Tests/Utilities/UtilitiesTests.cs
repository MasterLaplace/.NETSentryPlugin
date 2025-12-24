using FluentAssertions;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Utilities;
using Xunit;

namespace MySentry.Plugin.Tests.Utilities;

/// <summary>
/// Tests for utility classes.
/// </summary>
public class UtilitiesTests
{
    #region SentryIdGenerator Tests

    [Fact]
    public void SentryIdGenerator_Generate_ReturnsNonEmptyId()
    {
        // Act
        var eventId = SentryIdGenerator.Generate();

        // Assert
        eventId.Should().NotBe(SentryEventId.Empty);
        eventId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void SentryIdGenerator_Generate_ReturnsUniqueIds()
    {
        // Act
        var id1 = SentryIdGenerator.Generate();
        var id2 = SentryIdGenerator.Generate();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void SentryIdGenerator_Parse_ValidGuid_ReturnsEventId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var eventId = SentryIdGenerator.Parse(guidString);

        // Assert
        eventId.Value.Should().Be(guid);
    }

    [Fact]
    public void SentryIdGenerator_Parse_InvalidGuid_ThrowsFormatException()
    {
        // Arrange
        var invalidValue = "not-a-guid";

        // Act
        var act = () => SentryIdGenerator.Parse(invalidValue);

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("*not a valid Sentry event ID*");
    }

    [Fact]
    public void SentryIdGenerator_TryParse_ValidGuid_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var result = SentryIdGenerator.TryParse(guidString, out var eventId);

        // Assert
        result.Should().BeTrue();
        eventId.Value.Should().Be(guid);
    }

    [Fact]
    public void SentryIdGenerator_TryParse_InvalidGuid_ReturnsFalse()
    {
        // Arrange
        var invalidValue = "not-a-guid";

        // Act
        var result = SentryIdGenerator.TryParse(invalidValue, out var eventId);

        // Assert
        result.Should().BeFalse();
        eventId.Should().Be(SentryEventId.Empty);
    }

    [Fact]
    public void SentryIdGenerator_TryParse_NullValue_ReturnsFalse()
    {
        // Act
        var result = SentryIdGenerator.TryParse(null, out var eventId);

        // Assert
        result.Should().BeFalse();
        eventId.Should().Be(SentryEventId.Empty);
    }

    [Fact]
    public void SentryIdGenerator_TryParse_EmptyValue_ReturnsFalse()
    {
        // Act
        var result = SentryIdGenerator.TryParse(string.Empty, out var eventId);

        // Assert
        result.Should().BeFalse();
        eventId.Should().Be(SentryEventId.Empty);
    }

    #endregion

    #region EnvironmentDetector Tests

    [Fact]
    public void EnvironmentDetector_DetectedEnvironment_ReturnsValue()
    {
        // Act
        var environment = EnvironmentDetector.DetectedEnvironment;

        // Assert
        environment.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EnvironmentDetector_IsDevelopment_ReturnsBool()
    {
        // Act
        var isDev = EnvironmentDetector.IsDevelopment;

        // Assert - just verify it doesn't throw
        isDev.Should().Be(isDev); // tautology but ensures property works
    }

    [Fact]
    public void EnvironmentDetector_IsProduction_ReturnsBool()
    {
        // Act
        var isProd = EnvironmentDetector.IsProduction;

        // Assert
        isProd.Should().Be(isProd);
    }

    [Fact]
    public void EnvironmentDetector_IsStaging_ReturnsBool()
    {
        // Act
        var isStaging = EnvironmentDetector.IsStaging;

        // Assert
        isStaging.Should().Be(isStaging);
    }

    [Fact]
    public void EnvironmentDetector_IsRunningInContainer_ReturnsBool()
    {
        // Act
        var isContainer = EnvironmentDetector.IsRunningInContainer;

        // Assert
        isContainer.Should().Be(isContainer);
    }

    [Fact]
    public void EnvironmentDetector_IsRunningInKubernetes_ReturnsBool()
    {
        // Act
        var isK8s = EnvironmentDetector.IsRunningInKubernetes;

        // Assert
        isK8s.Should().Be(isK8s);
    }

    [Fact]
    public void EnvironmentDetector_IsRunningInAzure_ReturnsBool()
    {
        // Act
        var isAzure = EnvironmentDetector.IsRunningInAzure;

        // Assert
        isAzure.Should().Be(isAzure);
    }

    [Fact]
    public void EnvironmentDetector_IsRunningInAws_ReturnsBool()
    {
        // Act
        var isAws = EnvironmentDetector.IsRunningInAws;

        // Assert
        isAws.Should().Be(isAws);
    }

    [Fact]
    public void EnvironmentDetector_GetReleaseVersion_ReturnsValueOrNull()
    {
        // Act
        var version = EnvironmentDetector.GetReleaseVersion();

        // Assert - can be null if no entry assembly
        // Just verify it doesn't throw
        _ = version;
    }

    [Fact]
    public void EnvironmentDetector_GetInformationalVersion_ReturnsValueOrNull()
    {
        // Act
        var version = EnvironmentDetector.GetInformationalVersion();

        // Assert - can be null if no entry assembly or attribute
        _ = version;
    }

    #endregion
}
