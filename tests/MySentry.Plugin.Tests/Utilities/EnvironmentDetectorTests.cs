using FluentAssertions;
using MySentry.Plugin.Utilities;
using Xunit;

namespace MySentry.Plugin.Tests.Utilities;

/// <summary>
/// Tests for EnvironmentDetector.
/// Note: These tests are limited because actual environment detection depends on runtime conditions.
/// </summary>
public class EnvironmentDetectorTests
{
    [Fact]
    public void DetectedEnvironment_IsNotNull()
    {
        // Act
        var env = EnvironmentDetector.DetectedEnvironment;

        // Assert
        env.Should().NotBeNull();
    }

    [Fact]
    public void DetectedEnvironment_IsNotEmpty()
    {
        // Act
        var env = EnvironmentDetector.DetectedEnvironment;

        // Assert
        env.Should().NotBeEmpty();
    }

    [Fact]
    public void DetectedEnvironment_IsCached()
    {
        // Act
        var env1 = EnvironmentDetector.DetectedEnvironment;
        var env2 = EnvironmentDetector.DetectedEnvironment;

        // Assert - Same reference (cached)
        ReferenceEquals(env1, env2).Should().BeTrue();
    }

    [Fact]
    public void IsDevelopment_MatchesDetectedEnvironment()
    {
        // Act
        var isDev = EnvironmentDetector.IsDevelopment;
        var detected = EnvironmentDetector.DetectedEnvironment;

        // Assert
        if (detected.Equals("development", StringComparison.OrdinalIgnoreCase))
        {
            isDev.Should().BeTrue();
        }
        else
        {
            isDev.Should().BeFalse();
        }
    }

    [Fact]
    public void IsProduction_MatchesDetectedEnvironment()
    {
        // Act
        var isProd = EnvironmentDetector.IsProduction;
        var detected = EnvironmentDetector.DetectedEnvironment;

        // Assert
        if (detected.Equals("production", StringComparison.OrdinalIgnoreCase))
        {
            isProd.Should().BeTrue();
        }
        else
        {
            isProd.Should().BeFalse();
        }
    }

    [Fact]
    public void IsStaging_MatchesDetectedEnvironment()
    {
        // Act
        var isStaging = EnvironmentDetector.IsStaging;
        var detected = EnvironmentDetector.DetectedEnvironment;

        // Assert
        if (detected.Equals("staging", StringComparison.OrdinalIgnoreCase))
        {
            isStaging.Should().BeTrue();
        }
        else
        {
            isStaging.Should().BeFalse();
        }
    }

    [Fact]
    public void IsRunningInContainer_ReturnsBoolean()
    {
        // Act
        var result = EnvironmentDetector.IsRunningInContainer;

        // Assert - Just verify it returns without throwing
        result.Should().Be(result); // Tautology to verify it runs
    }

    [Fact]
    public void IsRunningInKubernetes_ReturnsBoolean()
    {
        // Act
        var result = EnvironmentDetector.IsRunningInKubernetes;

        // Assert
        result.Should().Be(result);
    }

    [Fact]
    public void IsRunningInAzure_ReturnsBoolean()
    {
        // Act
        var result = EnvironmentDetector.IsRunningInAzure;

        // Assert
        result.Should().Be(result);
    }

    [Fact]
    public void IsRunningInAws_ReturnsBoolean()
    {
        // Act
        var result = EnvironmentDetector.IsRunningInAws;

        // Assert
        result.Should().Be(result);
    }

    [Fact]
    public void GetReleaseVersion_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        var action = () => EnvironmentDetector.GetReleaseVersion();
        action.Should().NotThrow();
    }

    [Fact]
    public void GetInformationalVersion_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        var action = () => EnvironmentDetector.GetInformationalVersion();
        action.Should().NotThrow();
    }

    [Fact]
    public void AllEnvironmentChecks_DoNotThrow()
    {
        // Act & Assert - Just verify none of these throw
        var actions = new Action[]
        {
            () => _ = EnvironmentDetector.DetectedEnvironment,
            () => _ = EnvironmentDetector.IsDevelopment,
            () => _ = EnvironmentDetector.IsProduction,
            () => _ = EnvironmentDetector.IsStaging,
            () => _ = EnvironmentDetector.IsRunningInContainer,
            () => _ = EnvironmentDetector.IsRunningInKubernetes,
            () => _ = EnvironmentDetector.IsRunningInAzure,
            () => _ = EnvironmentDetector.IsRunningInAws,
            () => _ = EnvironmentDetector.GetReleaseVersion(),
            () => _ = EnvironmentDetector.GetInformationalVersion()
        };

        foreach (var action in actions)
        {
            action.Should().NotThrow();
        }
    }
}
