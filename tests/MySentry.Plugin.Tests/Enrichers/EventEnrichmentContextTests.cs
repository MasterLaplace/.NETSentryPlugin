using FluentAssertions;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Enrichers;
using Xunit;

using PluginSeverityLevel = MySentry.Plugin.Abstractions.SeverityLevel;
using PluginSentryUser = MySentry.Plugin.Abstractions.SentryUser;

namespace MySentry.Plugin.Tests.Enrichers;

/// <summary>
/// Tests for EventEnrichmentContext.
/// </summary>
public class EventEnrichmentContextTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithException_SetsProperties()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var level = PluginSeverityLevel.Error;

        // Act
        var context = new EventEnrichmentContext(exception, level);

        // Assert
        context.Exception.Should().Be(exception);
        context.Message.Should().BeNull();
        context.Level.Should().Be(level);
        context.User.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithException_DefaultLevelIsError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var context = new EventEnrichmentContext(exception);

        // Assert
        context.Level.Should().Be(PluginSeverityLevel.Error);
    }

    [Fact]
    public void Constructor_WithMessage_SetsProperties()
    {
        // Arrange
        var message = "Test message";
        var level = PluginSeverityLevel.Warning;

        // Act
        var context = new EventEnrichmentContext(message, level);

        // Assert
        context.Message.Should().Be(message);
        context.Exception.Should().BeNull();
        context.Level.Should().Be(level);
        context.User.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessage_DefaultLevelIsInfo()
    {
        // Arrange
        var message = "Test message";

        // Act
        var context = new EventEnrichmentContext(message);

        // Assert
        context.Level.Should().Be(PluginSeverityLevel.Info);
    }

    #endregion

    #region SetTag Tests

    [Fact]
    public void SetTag_AddsTag()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Act
        context.SetTag("environment", "production");

        // Assert
        context.Tags.Should().ContainKey("environment");
        context.Tags["environment"].Should().Be("production");
    }

    [Fact]
    public void SetTag_MultipleTags_AddsAll()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Act
        context.SetTag("env", "prod");
        context.SetTag("region", "eu-west");
        context.SetTag("version", "1.0.0");

        // Assert
        context.Tags.Should().HaveCount(3);
        context.Tags["env"].Should().Be("prod");
        context.Tags["region"].Should().Be("eu-west");
        context.Tags["version"].Should().Be("1.0.0");
    }

    [Fact]
    public void SetTag_SameKeyTwice_OverwritesValue()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Act
        context.SetTag("key", "value1");
        context.SetTag("key", "value2");

        // Assert
        context.Tags.Should().HaveCount(1);
        context.Tags["key"].Should().Be("value2");
    }

    #endregion

    #region SetExtra Tests

    [Fact]
    public void SetExtra_AddsExtra()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Act
        context.SetExtra("userId", "user123");

        // Assert
        context.Extras.Should().ContainKey("userId");
        context.Extras["userId"].Should().Be("user123");
    }

    [Fact]
    public void SetExtra_WithNullValue_AddsNull()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Act
        context.SetExtra("nullValue", null);

        // Assert
        context.Extras.Should().ContainKey("nullValue");
        context.Extras["nullValue"].Should().BeNull();
    }

    [Fact]
    public void SetExtra_WithComplexObject_AddsObject()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");
        var complexData = new { Id = 1, Name = "Test", Items = new[] { "a", "b" } };

        // Act
        context.SetExtra("data", complexData);

        // Assert
        context.Extras.Should().ContainKey("data");
        context.Extras["data"].Should().Be(complexData);
    }

    [Fact]
    public void SetExtra_SameKeyTwice_OverwritesValue()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Act
        context.SetExtra("key", "value1");
        context.SetExtra("key", "value2");

        // Assert
        context.Extras.Should().HaveCount(1);
        context.Extras["key"].Should().Be("value2");
    }

    #endregion

    #region SetContext Tests

    [Fact]
    public void SetContext_AddsContext()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");
        var runtimeContext = new { Name = ".NET 8", Version = "8.0.0" };

        // Act
        context.SetContext("Runtime", runtimeContext);

        // Assert
        context.Contexts.Should().ContainKey("Runtime");
        context.Contexts["Runtime"].Should().Be(runtimeContext);
    }

    [Fact]
    public void SetContext_MultipleContexts_AddsAll()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");
        var runtime = new { Name = ".NET" };
        var device = new { Name = "Server", Memory = "16GB" };

        // Act
        context.SetContext("Runtime", runtime);
        context.SetContext("Device", device);

        // Assert
        context.Contexts.Should().HaveCount(2);
        context.Contexts["Runtime"].Should().Be(runtime);
        context.Contexts["Device"].Should().Be(device);
    }

    [Fact]
    public void SetContext_SameKeyTwice_OverwritesValue()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");
        var context1 = new { Version = "1.0" };
        var context2 = new { Version = "2.0" };

        // Act
        context.SetContext("App", context1);
        context.SetContext("App", context2);

        // Assert
        context.Contexts.Should().HaveCount(1);
        context.Contexts["App"].Should().Be(context2);
    }

    #endregion

    #region User Tests

    [Fact]
    public void User_CanBeSet()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");
        var user = new PluginSentryUser
        {
            Id = "user123",
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        context.User = user;

        // Assert
        context.User.Should().NotBeNull();
        context.User!.Id.Should().Be("user123");
        context.User.Username.Should().Be("testuser");
        context.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void User_CanBeSetToNull()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");
        context.User = new PluginSentryUser { Id = "test" };

        // Act
        context.User = null;

        // Assert
        context.User.Should().BeNull();
    }

    #endregion

    #region Collections Are ReadOnly Tests

    [Fact]
    public void Tags_IsReadOnly()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Assert
        context.Tags.Should().BeAssignableTo<IReadOnlyDictionary<string, string>>();
    }

    [Fact]
    public void Extras_IsReadOnly()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Assert
        context.Extras.Should().BeAssignableTo<IReadOnlyDictionary<string, object?>>();
    }

    [Fact]
    public void Contexts_IsReadOnly()
    {
        // Arrange
        var context = new EventEnrichmentContext("test");

        // Assert
        context.Contexts.Should().BeAssignableTo<IReadOnlyDictionary<string, object>>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullEnrichment_AddsAllData()
    {
        // Arrange
        var exception = new InvalidOperationException("Operation failed");
        var context = new EventEnrichmentContext(exception, PluginSeverityLevel.Error);

        // Act
        context.SetTag("environment", "production");
        context.SetTag("version", "2.0.0");
        context.SetExtra("correlationId", "abc-123");
        context.SetExtra("requestPath", "/api/users");
        context.SetContext("Runtime", new { Name = ".NET 8" });
        context.SetContext("Browser", new { Name = "Chrome" });
        context.User = new PluginSentryUser { Id = "user1", Username = "admin" };

        // Assert
        context.Exception.Should().Be(exception);
        context.Level.Should().Be(PluginSeverityLevel.Error);
        context.Tags.Should().HaveCount(2);
        context.Extras.Should().HaveCount(2);
        context.Contexts.Should().HaveCount(2);
        context.User.Should().NotBeNull();
        context.User!.Id.Should().Be("user1");
    }

    #endregion
}
