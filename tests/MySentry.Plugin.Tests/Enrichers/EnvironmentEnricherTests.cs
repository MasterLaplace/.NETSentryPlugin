using FluentAssertions;
using MySentry.Plugin.Enrichers;
using Xunit;

namespace MySentry.Plugin.Tests.Enrichers;

/// <summary>
/// Tests for EnvironmentEnricher.
/// </summary>
public class EnvironmentEnricherTests
{
    [Fact]
    public void Order_Returns50()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();

        // Act & Assert
        enricher.Order.Should().Be(50);
    }

    [Fact]
    public void Enrich_SetsRuntimeContext()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();
        var context = new EventEnrichmentContext("test");

        // Act
        enricher.Enrich(context);

        // Assert
        context.Contexts.Should().ContainKey("Runtime");
        var runtime = context.Contexts["Runtime"];
        runtime.Should().NotBeNull();
    }

    [Fact]
    public void Enrich_SetsAppContext()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();
        var context = new EventEnrichmentContext("test");

        // Act
        enricher.Enrich(context);

        // Assert
        context.Contexts.Should().ContainKey("App");
        var app = context.Contexts["App"];
        app.Should().NotBeNull();
    }

    [Fact]
    public void Enrich_SetsRuntimeTag()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();
        var context = new EventEnrichmentContext("test");

        // Act
        enricher.Enrich(context);

        // Assert
        context.Tags.Should().ContainKey("runtime");
        context.Tags["runtime"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Enrich_SetsOsTag()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();
        var context = new EventEnrichmentContext("test");

        // Act
        enricher.Enrich(context);

        // Assert
        context.Tags.Should().ContainKey("os");
        context.Tags["os"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Enrich_WithExceptionContext_AddsContexts()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();
        var exception = new InvalidOperationException("Test");
        var context = new EventEnrichmentContext(exception);

        // Act
        enricher.Enrich(context);

        // Assert
        context.Contexts.Should().HaveCount(2);
        context.Tags.Should().HaveCount(2);
    }

    [Fact]
    public void Enrich_DoesNotOverwriteExistingTags()
    {
        // Arrange
        var enricher = new EnvironmentEnricher();
        var context = new EventEnrichmentContext("test");
        context.SetTag("custom", "value");

        // Act
        enricher.Enrich(context);

        // Assert
        context.Tags.Should().ContainKey("custom");
        context.Tags["custom"].Should().Be("value");
    }
}
