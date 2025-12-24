using FluentAssertions;
using MySentry.Plugin.Abstractions;
using Xunit;

namespace MySentry.Plugin.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="SentryEventId"/>.
/// </summary>
public class SentryEventIdTests
{
    [Fact]
    public void Empty_ReturnsEmptyGuid()
    {
        // Arrange & Act
        var empty = SentryEventId.Empty;

        // Assert
        empty.Value.Should().Be(Guid.Empty);
        empty.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Create_ReturnsNewUniqueId()
    {
        // Act
        var id1 = SentryEventId.Create();
        var id2 = SentryEventId.Create();

        // Assert
        id1.Should().NotBe(SentryEventId.Empty);
        id2.Should().NotBe(SentryEventId.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Constructor_SetsGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var eventId = new SentryEventId(guid);

        // Assert
        eventId.Value.Should().Be(guid);
    }

    [Fact]
    public void IsEmpty_WhenNonEmpty_ReturnsFalse()
    {
        // Arrange
        var eventId = new SentryEventId(Guid.NewGuid());

        // Assert
        eventId.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsHexString()
    {
        // Arrange
        var guid = new Guid("12345678-1234-1234-1234-123456789abc");
        var eventId = new SentryEventId(guid);

        // Act
        var result = eventId.ToString();

        // Assert
        result.Should().Be("12345678123412341234123456789abc");
    }

    [Fact]
    public void ImplicitConversion_ToGuid_Works()
    {
        // Arrange
        var originalGuid = Guid.NewGuid();
        var eventId = new SentryEventId(originalGuid);

        // Act
        Guid converted = eventId;

        // Assert
        converted.Should().Be(originalGuid);
    }

    [Fact]
    public void ImplicitConversion_FromGuid_Works()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        SentryEventId eventId = guid;

        // Assert
        eventId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new SentryEventId(guid);
        var id2 = new SentryEventId(guid);

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        // Arrange
        var id1 = new SentryEventId(Guid.NewGuid());
        var id2 = new SentryEventId(Guid.NewGuid());

        // Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void Empty_IsEmpty_ReturnsTrue()
    {
        // Arrange & Act
        var empty = SentryEventId.Empty;

        // Assert
        empty.IsEmpty.Should().BeTrue();
    }
}
