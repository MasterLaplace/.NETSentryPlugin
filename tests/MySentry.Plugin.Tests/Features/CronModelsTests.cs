using FluentAssertions;
using MySentry.Plugin.Features.Crons;
using Xunit;
using PluginCheckInStatus = MySentry.Plugin.Features.Crons.CheckInStatus;

namespace MySentry.Plugin.Tests.Features;

/// <summary>
/// Tests for Cron monitoring models.
/// </summary>
public class CronModelsTests
{
    #region CheckInStatus Tests

    [Fact]
    public void CheckInStatus_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<PluginCheckInStatus>().Should().HaveCount(3);
        PluginCheckInStatus.InProgress.Should().BeDefined();
        PluginCheckInStatus.Ok.Should().BeDefined();
        PluginCheckInStatus.Error.Should().BeDefined();
    }

    [Fact]
    public void CheckInStatus_InProgress_HasCorrectValue()
    {
        // Assert
        ((int)PluginCheckInStatus.InProgress).Should().Be(0);
    }

    [Fact]
    public void CheckInStatus_Ok_HasCorrectValue()
    {
        // Assert
        ((int)PluginCheckInStatus.Ok).Should().Be(1);
    }

    [Fact]
    public void CheckInStatus_Error_HasCorrectValue()
    {
        // Assert
        ((int)PluginCheckInStatus.Error).Should().Be(2);
    }

    #endregion

    #region CronJobConfig Tests

    [Fact]
    public void CronJobConfig_RequiredProperties_MustBeSet()
    {
        // Arrange & Act
        var config = new CronJobConfig
        {
            MonitorSlug = "test-job"
        };

        // Assert
        config.MonitorSlug.Should().Be("test-job");
    }

    [Fact]
    public void CronJobConfig_OptionalProperties_HaveDefaults()
    {
        // Arrange & Act
        var config = new CronJobConfig
        {
            MonitorSlug = "test-job"
        };

        // Assert
        config.Schedule.Should().BeNull();
        config.ScheduleType.Should().Be(ScheduleType.Crontab);
        config.CheckInMarginMinutes.Should().BeNull();
        config.MaxRuntimeMinutes.Should().BeNull();
        config.Timezone.Should().BeNull();
    }

    [Fact]
    public void CronJobConfig_AllProperties_CanBeSet()
    {
        // Arrange & Act
        var config = new CronJobConfig
        {
            MonitorSlug = "daily-cleanup",
            Schedule = "0 0 * * *",
            ScheduleType = ScheduleType.Crontab,
            CheckInMarginMinutes = 5,
            MaxRuntimeMinutes = 30,
            Timezone = "UTC"
        };

        // Assert
        config.MonitorSlug.Should().Be("daily-cleanup");
        config.Schedule.Should().Be("0 0 * * *");
        config.ScheduleType.Should().Be(ScheduleType.Crontab);
        config.CheckInMarginMinutes.Should().Be(5);
        config.MaxRuntimeMinutes.Should().Be(30);
        config.Timezone.Should().Be("UTC");
    }

    #endregion

    #region ScheduleType Tests

    [Fact]
    public void ScheduleType_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ScheduleType>().Should().HaveCount(2);
        ScheduleType.Crontab.Should().BeDefined();
        ScheduleType.Interval.Should().BeDefined();
    }

    #endregion
}
