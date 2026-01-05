using FluentAssertions;
using MySentry.Plugin.Abstractions;
using Xunit;

using PluginSentryUser = MySentry.Plugin.Abstractions.SentryUser;

namespace MySentry.Plugin.Tests.Abstractions;

/// <summary>
/// Tests for SentryUser.
/// Note: The actual type is PluginSentryUser in the global using.
/// </summary>
public class SentryUserTests
{
    #region Constructor Tests

    [Fact]
    public void DefaultConstructor_AllPropertiesAreNull()
    {
        // Arrange & Act
        var user = new PluginSentryUser();

        // Assert
        user.Id.Should().BeNull();
        user.Email.Should().BeNull();
        user.Username.Should().BeNull();
        user.IpAddress.Should().BeNull();
        user.Segment.Should().BeNull();
        user.AdditionalData.Should().NotBeNull();
        user.AdditionalData.Should().BeEmpty();
    }

    [Fact]
    public void ConstructorWithId_SetsId()
    {
        // Arrange & Act
        var user = new PluginSentryUser("user-123");

        // Assert
        user.Id.Should().Be("user-123");
        user.Email.Should().BeNull();
        user.Username.Should().BeNull();
    }

    [Fact]
    public void ConstructorWithIdAndEmail_SetsBoth()
    {
        // Arrange & Act
        var user = new PluginSentryUser("user-123", "test@example.com");

        // Assert
        user.Id.Should().Be("user-123");
        user.Email.Should().Be("test@example.com");
        user.Username.Should().BeNull();
    }

    [Fact]
    public void ConstructorWithAllParams_SetsAll()
    {
        // Arrange & Act
        var user = new PluginSentryUser("user-123", "test@example.com", "testuser");

        // Assert
        user.Id.Should().Be("user-123");
        user.Email.Should().Be("test@example.com");
        user.Username.Should().Be("testuser");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_CanBeSet()
    {
        // Arrange
        var user = new PluginSentryUser();

        // Act
        user.Id = "user-456";

        // Assert
        user.Id.Should().Be("user-456");
    }

    [Fact]
    public void Email_CanBeSet()
    {
        // Arrange
        var user = new PluginSentryUser();

        // Act
        user.Email = "user@test.com";

        // Assert
        user.Email.Should().Be("user@test.com");
    }

    [Fact]
    public void Username_CanBeSet()
    {
        // Arrange
        var user = new PluginSentryUser();

        // Act
        user.Username = "john_doe";

        // Assert
        user.Username.Should().Be("john_doe");
    }

    [Fact]
    public void IpAddress_CanBeSet()
    {
        // Arrange
        var user = new PluginSentryUser();

        // Act
        user.IpAddress = "192.168.1.1";

        // Assert
        user.IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public void Segment_CanBeSet()
    {
        // Arrange
        var user = new PluginSentryUser();

        // Act
        user.Segment = "enterprise";

        // Assert
        user.Segment.Should().Be("enterprise");
    }

    #endregion

    #region Fluent Methods Tests

    [Fact]
    public void WithData_AddsDataAndReturnsUser()
    {
        // Arrange
        var user = new PluginSentryUser("user-123");

        // Act
        var result = user.WithData("role", "admin");

        // Assert
        result.Should().BeSameAs(user);
        user.AdditionalData.Should().ContainKey("role");
        user.AdditionalData["role"].Should().Be("admin");
    }

    [Fact]
    public void WithData_MultipleCalls_AddsAllData()
    {
        // Arrange
        var user = new PluginSentryUser("user-123");

        // Act
        user.WithData("role", "admin")
            .WithData("department", "IT")
            .WithData("location", "Paris");

        // Assert
        user.AdditionalData.Should().HaveCount(3);
        user.AdditionalData["role"].Should().Be("admin");
        user.AdditionalData["department"].Should().Be("IT");
        user.AdditionalData["location"].Should().Be("Paris");
    }

    [Fact]
    public void WithIpAddress_SetsIpAddressAndReturnsUser()
    {
        // Arrange
        var user = new PluginSentryUser("user-123");

        // Act
        var result = user.WithIpAddress("10.0.0.1");

        // Assert
        result.Should().BeSameAs(user);
        user.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public void WithAutoIpAddress_SetsAutoIpAndReturnsUser()
    {
        // Arrange
        var user = new PluginSentryUser("user-123");

        // Act
        var result = user.WithAutoIpAddress();

        // Assert
        result.Should().BeSameAs(user);
        user.IpAddress.Should().Be("{{auto}}");
    }

    [Fact]
    public void WithSegment_SetsSegmentAndReturnsUser()
    {
        // Arrange
        var user = new PluginSentryUser("user-123");

        // Act
        var result = user.WithSegment("premium");

        // Assert
        result.Should().BeSameAs(user);
        user.Segment.Should().Be("premium");
    }

    [Fact]
    public void FluentChaining_AllMethods()
    {
        // Arrange & Act
        var user = new PluginSentryUser("user-123")
            .WithData("role", "admin")
            .WithIpAddress("192.168.0.1")
            .WithSegment("enterprise");

        // Assert
        user.Id.Should().Be("user-123");
        user.IpAddress.Should().Be("192.168.0.1");
        user.Segment.Should().Be("enterprise");
        user.AdditionalData["role"].Should().Be("admin");
    }

    #endregion

    #region AdditionalData Tests

    [Fact]
    public void AdditionalData_SameKeyTwice_OverwritesValue()
    {
        // Arrange
        var user = new PluginSentryUser();

        // Act
        user.WithData("key", "value1");
        user.WithData("key", "value2");

        // Assert
        user.AdditionalData.Should().HaveCount(1);
        user.AdditionalData["key"].Should().Be("value2");
    }

    [Fact]
    public void AdditionalData_IsInitialized()
    {
        // Arrange & Act
        var user = new PluginSentryUser();

        // Assert - AdditionalData is initialized even before any WithData call
        user.AdditionalData.Should().NotBeNull();
    }

    #endregion
}
