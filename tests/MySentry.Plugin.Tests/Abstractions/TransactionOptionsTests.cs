using FluentAssertions;
using MySentry.Plugin.Abstractions;
using Xunit;

namespace MySentry.Plugin.Tests.Abstractions;

/// <summary>
/// Tests for TransactionOptions.
/// </summary>
public class TransactionOptionsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new TransactionOptions();

        // Assert
        options.Description.Should().BeNull();
        options.BindToScope.Should().BeTrue();
        options.IsSampled.Should().BeNull();
        options.Tags.Should().NotBeNull();
        options.Tags.Should().BeEmpty();
        options.ExtraData.Should().NotBeNull();
        options.ExtraData.Should().BeEmpty();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Description_CanBeSet()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.Description = "User checkout transaction";

        // Assert
        options.Description.Should().Be("User checkout transaction");
    }

    [Fact]
    public void BindToScope_CanBeDisabled()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.BindToScope = false;

        // Assert
        options.BindToScope.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsSampled_CanBeSet(bool sampled)
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.IsSampled = sampled;

        // Assert
        options.IsSampled.Should().Be(sampled);
    }

    [Fact]
    public void IsSampled_CanBeSetToNull()
    {
        // Arrange
        var options = new TransactionOptions { IsSampled = true };

        // Act
        options.IsSampled = null;

        // Assert
        options.IsSampled.Should().BeNull();
    }

    #endregion

    #region WithTag Tests

    [Fact]
    public void WithTag_AddsTagAndReturnsOptions()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        var result = options.WithTag("environment", "production");

        // Assert
        result.Should().BeSameAs(options);
        options.Tags.Should().ContainKey("environment");
        options.Tags["environment"].Should().Be("production");
    }

    [Fact]
    public void WithTag_MultipleTags_AddsAll()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.WithTag("env", "prod")
               .WithTag("region", "eu-west")
               .WithTag("version", "1.0.0");

        // Assert
        options.Tags.Should().HaveCount(3);
        options.Tags["env"].Should().Be("prod");
        options.Tags["region"].Should().Be("eu-west");
        options.Tags["version"].Should().Be("1.0.0");
    }

    [Fact]
    public void WithTag_SameKeyTwice_OverwritesValue()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.WithTag("key", "value1").WithTag("key", "value2");

        // Assert
        options.Tags.Should().HaveCount(1);
        options.Tags["key"].Should().Be("value2");
    }

    #endregion

    #region WithExtra Tests

    [Fact]
    public void WithExtra_AddsExtraAndReturnsOptions()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        var result = options.WithExtra("userId", "user-123");

        // Assert
        result.Should().BeSameAs(options);
        options.ExtraData.Should().ContainKey("userId");
        options.ExtraData["userId"].Should().Be("user-123");
    }

    [Fact]
    public void WithExtra_WithNullValue_AddsNull()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.WithExtra("nullValue", null);

        // Assert
        options.ExtraData.Should().ContainKey("nullValue");
        options.ExtraData["nullValue"].Should().BeNull();
    }

    [Fact]
    public void WithExtra_WithComplexObject_AddsObject()
    {
        // Arrange
        var options = new TransactionOptions();
        var complexData = new { Id = 1, Name = "Test" };

        // Act
        options.WithExtra("data", complexData);

        // Assert
        options.ExtraData.Should().ContainKey("data");
        options.ExtraData["data"].Should().Be(complexData);
    }

    [Fact]
    public void WithExtra_MultipleCalls_AddsAll()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.WithExtra("key1", "value1")
               .WithExtra("key2", 42)
               .WithExtra("key3", true);

        // Assert
        options.ExtraData.Should().HaveCount(3);
        options.ExtraData["key1"].Should().Be("value1");
        options.ExtraData["key2"].Should().Be(42);
        options.ExtraData["key3"].Should().Be(true);
    }

    [Fact]
    public void WithExtra_SameKeyTwice_OverwritesValue()
    {
        // Arrange
        var options = new TransactionOptions();

        // Act
        options.WithExtra("key", "value1").WithExtra("key", "value2");

        // Assert
        options.ExtraData.Should().HaveCount(1);
        options.ExtraData["key"].Should().Be("value2");
    }

    #endregion

    #region Fluent Chaining Tests

    [Fact]
    public void FluentChaining_AllMethods()
    {
        // Arrange & Act
        var options = new TransactionOptions()
            .WithTag("env", "prod")
            .WithTag("version", "1.0")
            .WithExtra("userId", "123")
            .WithExtra("action", "checkout");

        // Assert
        options.Tags.Should().HaveCount(2);
        options.ExtraData.Should().HaveCount(2);
    }

    [Fact]
    public void MixedConfiguration_PropertiesAndFluent()
    {
        // Arrange & Act
        var options = new TransactionOptions
        {
            Description = "Payment transaction",
            BindToScope = false,
            IsSampled = true
        };
        options.WithTag("type", "payment")
               .WithExtra("amount", 99.99m);

        // Assert
        options.Description.Should().Be("Payment transaction");
        options.BindToScope.Should().BeFalse();
        options.IsSampled.Should().BeTrue();
        options.Tags["type"].Should().Be("payment");
        options.ExtraData["amount"].Should().Be(99.99m);
    }

    #endregion
}
