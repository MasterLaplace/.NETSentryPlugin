using FluentAssertions;
using MySentry.Plugin.Configuration;
using Xunit;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for FilteringOptions.
/// </summary>
public class FilteringOptionsTests
{
    [Fact]
    public void IgnoreExceptionTypes_HasDefaultEntries()
    {
        // Arrange & Act
        var options = new FilteringOptions();

        // Assert
        options.IgnoreExceptionTypes.Should().NotBeEmpty();
        options.IgnoreExceptionTypes.Should().Contain("System.OperationCanceledException");
        options.IgnoreExceptionTypes.Should().Contain("System.Threading.Tasks.TaskCanceledException");
    }

    [Fact]
    public void IgnoreUrls_IsEmptyByDefault()
    {
        // Arrange & Act
        var options = new FilteringOptions();

        // Assert
        options.IgnoreUrls.Should().NotBeNull();
        options.IgnoreUrls.Should().BeEmpty();
    }

    [Fact]
    public void IgnoreStatusCodes_HasDefault404()
    {
        // Arrange & Act
        var options = new FilteringOptions();

        // Assert
        options.IgnoreStatusCodes.Should().NotBeEmpty();
        options.IgnoreStatusCodes.Should().Contain(404);
    }

    [Fact]
    public void IgnoreMessages_IsEmptyByDefault()
    {
        // Arrange & Act
        var options = new FilteringOptions();

        // Assert
        options.IgnoreMessages.Should().NotBeNull();
        options.IgnoreMessages.Should().BeEmpty();
    }

    [Fact]
    public void IgnoreUserAgents_HasDefaultEntries()
    {
        // Arrange & Act
        var options = new FilteringOptions();

        // Assert
        options.IgnoreUserAgents.Should().NotBeEmpty();
        options.IgnoreUserAgents.Should().Contain("health*");
        options.IgnoreUserAgents.Should().Contain("kube-probe/*");
        options.IgnoreUserAgents.Should().Contain("GoogleHC/*");
        options.IgnoreUserAgents.Should().Contain("ELB-HealthChecker/*");
    }

    [Fact]
    public void IgnoreExceptionTypes_CanBeModified()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreExceptionTypes.Add("MyCustomException");

        // Assert
        options.IgnoreExceptionTypes.Should().Contain("MyCustomException");
    }

    [Fact]
    public void IgnoreUrls_CanBeModified()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreUrls.Add("/api/internal/*");

        // Assert
        options.IgnoreUrls.Should().Contain("/api/internal/*");
    }

    [Fact]
    public void IgnoreStatusCodes_CanBeModified()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreStatusCodes.Add(503);
        options.IgnoreStatusCodes.Add(429);

        // Assert
        options.IgnoreStatusCodes.Should().Contain(503);
        options.IgnoreStatusCodes.Should().Contain(429);
    }

    [Fact]
    public void IgnoreMessages_CanBeModified()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreMessages.Add("Connection timeout*");

        // Assert
        options.IgnoreMessages.Should().Contain("Connection timeout*");
    }

    [Fact]
    public void IgnoreUserAgents_CanBeModified()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreUserAgents.Add("MyBot/*");

        // Assert
        options.IgnoreUserAgents.Should().Contain("MyBot/*");
    }

    [Fact]
    public void IgnoreExceptionTypes_CanBeCleared()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreExceptionTypes.Clear();

        // Assert
        options.IgnoreExceptionTypes.Should().BeEmpty();
    }

    [Fact]
    public void IgnoreStatusCodes_CanBeReplaced()
    {
        // Arrange
        var options = new FilteringOptions();

        // Act
        options.IgnoreStatusCodes = new List<int> { 500, 502, 503 };

        // Assert
        options.IgnoreStatusCodes.Should().HaveCount(3);
        options.IgnoreStatusCodes.Should().Contain(500);
        options.IgnoreStatusCodes.Should().NotContain(404);
    }
}
