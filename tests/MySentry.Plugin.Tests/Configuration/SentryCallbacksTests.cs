using FluentAssertions;
using MySentry.Plugin.Abstractions;
using MySentry.Plugin.Configuration;
using Xunit;

using PluginSeverityLevel = MySentry.Plugin.Abstractions.SeverityLevel;
using PluginBreadcrumbLevel = MySentry.Plugin.Abstractions.BreadcrumbLevel;

namespace MySentry.Plugin.Tests.Configuration;

/// <summary>
/// Tests for SentryCallbacks types.
/// </summary>
public class SentryCallbacksTests
{
    #region EventProcessingInfo Tests

    [Fact]
    public void EventProcessingInfo_CanBeInstantiated()
    {
        // Act
        var info = new EventProcessingInfo();

        // Assert
        info.Should().NotBeNull();
    }

    [Fact]
    public void EventProcessingInfo_Message_CanBeSetAndGet()
    {
        // Arrange
        var info = new EventProcessingInfo();

        // Act
        info.Message = "Test error message";

        // Assert
        info.Message.Should().Be("Test error message");
    }

    [Fact]
    public void EventProcessingInfo_Tags_DefaultsToEmptyDictionary()
    {
        // Act
        var info = new EventProcessingInfo();

        // Assert
        info.Tags.Should().NotBeNull();
        info.Tags.Should().BeEmpty();
    }

    [Fact]
    public void EventProcessingInfo_Tags_CanAddAndRemove()
    {
        // Arrange
        var info = new EventProcessingInfo();

        // Act
        info.Tags["environment"] = "production";
        info.Tags["version"] = "1.0.0";

        // Assert
        info.Tags.Should().HaveCount(2);
        info.Tags["environment"].Should().Be("production");
        info.Tags["version"].Should().Be("1.0.0");

        // Act - Remove
        info.Tags.Remove("version");

        // Assert
        info.Tags.Should().HaveCount(1);
        info.Tags.Should().NotContainKey("version");
    }

    [Fact]
    public void EventProcessingInfo_Extra_DefaultsToEmptyDictionary()
    {
        // Act
        var info = new EventProcessingInfo();

        // Assert
        info.Extra.Should().NotBeNull();
        info.Extra.Should().BeEmpty();
    }

    [Fact]
    public void EventProcessingInfo_Extra_CanAddComplexTypes()
    {
        // Arrange
        var info = new EventProcessingInfo();

        // Act
        info.Extra["user"] = new { Id = 123, Name = "John" };
        info.Extra["count"] = 42;

        // Assert
        info.Extra.Should().HaveCount(2);
    }

    [Fact]
    public void EventProcessingInfo_Exception_CanBeSet()
    {
        // Arrange
        var info = new EventProcessingInfo();
        var exception = new InvalidOperationException("Test");

        // Act
        info.Exception = exception;

        // Assert
        info.Exception.Should().Be(exception);
        info.Exception?.GetType().FullName.Should().Be("System.InvalidOperationException");
    }

    [Fact]
    public void EventProcessingInfo_Level_CanBeSet()
    {
        // Arrange
        var info = new EventProcessingInfo();

        // Act
        info.Level = PluginSeverityLevel.Error;

        // Assert
        info.Level.Should().Be(PluginSeverityLevel.Error);
    }

    [Fact]
    public void EventProcessingInfo_TransactionName_CanBeSet()
    {
        // Arrange
        var info = new EventProcessingInfo();

        // Act
        info.TransactionName = "GET /api/users";

        // Assert
        info.TransactionName.Should().Be("GET /api/users");
    }

    [Fact]
    public void EventProcessingInfo_Environment_CanBeSet()
    {
        // Arrange
        var info = new EventProcessingInfo();

        // Act
        info.Environment = "production";

        // Assert
        info.Environment.Should().Be("production");
    }

    #endregion

    #region BreadcrumbProcessingInfo Tests

    [Fact]
    public void BreadcrumbProcessingInfo_CanBeInstantiated()
    {
        // Act
        var info = new BreadcrumbProcessingInfo();

        // Assert
        info.Should().NotBeNull();
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Type_CanBeSetAndGet()
    {
        // Arrange
        var info = new BreadcrumbProcessingInfo();

        // Act
        info.Type = "http";

        // Assert
        info.Type.Should().Be("http");
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Category_CanBeSetAndGet()
    {
        // Arrange
        var info = new BreadcrumbProcessingInfo();

        // Act
        info.Category = "api.call";

        // Assert
        info.Category.Should().Be("api.call");
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Message_CanBeSetAndGet()
    {
        // Arrange
        var info = new BreadcrumbProcessingInfo();

        // Act
        info.Message = "User clicked button";

        // Assert
        info.Message.Should().Be("User clicked button");
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Level_CanBeSetAndGet()
    {
        // Arrange
        var info = new BreadcrumbProcessingInfo();

        // Act
        info.Level = PluginBreadcrumbLevel.Info;

        // Assert
        info.Level.Should().Be(PluginBreadcrumbLevel.Info);
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Data_DefaultsToEmptyDictionary()
    {
        // Act
        var info = new BreadcrumbProcessingInfo();

        // Assert
        info.Data.Should().NotBeNull();
        info.Data.Should().BeEmpty();
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Data_CanAddMultipleItems()
    {
        // Arrange
        var info = new BreadcrumbProcessingInfo();

        // Act
        info.Data["url"] = "https://api.example.com";
        info.Data["status_code"] = "200";
        info.Data["method"] = "GET";

        // Assert
        info.Data.Should().HaveCount(3);
        info.Data["url"].Should().Be("https://api.example.com");
    }

    [Fact]
    public void BreadcrumbProcessingInfo_Timestamp_CanBeSet()
    {
        // Arrange
        var info = new BreadcrumbProcessingInfo();
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        info.Timestamp = timestamp;

        // Assert
        info.Timestamp.Should().Be(timestamp);
    }

    #endregion

    #region PluginTransactionSamplingContext Tests

    [Fact]
    public void PluginTransactionSamplingContext_CanBeInstantiated()
    {
        // Act
        var context = new PluginTransactionSamplingContext
        {
            TransactionName = "GET /api/users",
            Operation = "http.server"
        };

        // Assert
        context.Should().NotBeNull();
        context.TransactionName.Should().Be("GET /api/users");
        context.Operation.Should().Be("http.server");
    }

    [Fact]
    public void PluginTransactionSamplingContext_ParentSampled_DefaultsToNull()
    {
        // Act
        var context = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test"
        };

        // Assert
        context.ParentSampled.Should().BeNull();
    }

    [Fact]
    public void PluginTransactionSamplingContext_IsRoot_WhenParentSampledIsNull()
    {
        // Act
        var context = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test",
            ParentSampled = null
        };

        // Assert
        context.IsRoot.Should().BeTrue();
    }

    [Fact]
    public void PluginTransactionSamplingContext_IsNotRoot_WhenParentSampledHasValue()
    {
        // Act
        var context = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test",
            ParentSampled = true
        };

        // Assert
        context.IsRoot.Should().BeFalse();
    }

    [Fact]
    public void PluginTransactionSamplingContext_CustomData_DefaultsToEmptyDictionary()
    {
        // Act
        var context = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test"
        };

        // Assert
        context.CustomData.Should().NotBeNull();
        context.CustomData.Should().BeEmpty();
    }

    #endregion

    #region Callback Delegate Tests

    [Fact]
    public void BeforeSendCallback_CanFilterEvents()
    {
        // Arrange
        SentryCallbacks.BeforeSendCallback callback = info =>
        {
            // Filter health check events
            if (info.TransactionName?.Contains("health") == true)
                return false;
            return true;
        };

        // Assert
        var healthEvent = new EventProcessingInfo { TransactionName = "/health" };
        callback(healthEvent).Should().BeFalse();

        var apiEvent = new EventProcessingInfo { TransactionName = "/api/users" };
        callback(apiEvent).Should().BeTrue();
    }

    [Fact]
    public void BeforeSendCallback_CanScrubTags()
    {
        // Arrange
        SentryCallbacks.BeforeSendCallback callback = info =>
        {
            var sensitiveKeys = info.Tags.Keys
                .Where(k => k.Contains("password") || k.Contains("secret"))
                .ToList();

            foreach (var key in sensitiveKeys)
            {
                info.Tags.Remove(key);
            }

            return true;
        };

        var eventInfo = new EventProcessingInfo();
        eventInfo.Tags["user"] = "john";
        eventInfo.Tags["password"] = "secret123";
        eventInfo.Tags["api_secret"] = "abc";

        // Act
        var shouldSend = callback(eventInfo);

        // Assert
        shouldSend.Should().BeTrue();
        eventInfo.Tags.Should().ContainKey("user");
        eventInfo.Tags.Should().NotContainKey("password");
        eventInfo.Tags.Should().NotContainKey("api_secret");
    }

    [Fact]
    public void BeforeBreadcrumbCallback_CanFilterByCategory()
    {
        // Arrange
        var noisyCategories = new HashSet<string> { "console", "debug", "heartbeat" };

        SentryCallbacks.BeforeBreadcrumbCallback callback = info =>
        {
            if (info.Category != null && noisyCategories.Contains(info.Category))
                return false;
            return true;
        };

        // Assert
        callback(new BreadcrumbProcessingInfo { Category = "console" }).Should().BeFalse();
        callback(new BreadcrumbProcessingInfo { Category = "debug" }).Should().BeFalse();
        callback(new BreadcrumbProcessingInfo { Category = "http" }).Should().BeTrue();
    }

    [Fact]
    public void TracesSamplerCallback_CanReturnNull()
    {
        // Arrange
        SentryCallbacks.TracesSamplerCallback sampler = context => null;

        // Act
        var context = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test"
        };
        var result = sampler(context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TracesSamplerCallback_CanReturnSampleRate()
    {
        // Arrange
        SentryCallbacks.TracesSamplerCallback sampler = context =>
        {
            if (context.TransactionName.Contains("health"))
                return 0.0;
            if (context.Operation == "http.server")
                return 1.0;
            return 0.5;
        };

        // Assert
        var healthContext = new PluginTransactionSamplingContext
        {
            TransactionName = "/health",
            Operation = "http.server"
        };
        sampler(healthContext).Should().Be(0.0);

        var httpContext = new PluginTransactionSamplingContext
        {
            TransactionName = "/api/users",
            Operation = "http.server"
        };
        sampler(httpContext).Should().Be(1.0);

        var dbContext = new PluginTransactionSamplingContext
        {
            TransactionName = "SELECT users",
            Operation = "db.query"
        };
        sampler(dbContext).Should().Be(0.5);
    }

    [Fact]
    public void TracesSamplerCallback_CanInheritParentDecision()
    {
        // Arrange
        SentryCallbacks.TracesSamplerCallback sampler = context =>
        {
            if (context.ParentSampled.HasValue)
                return context.ParentSampled.Value ? 1.0 : 0.0;
            return 0.5;
        };

        // Assert
        var sampledParent = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test",
            ParentSampled = true
        };
        sampler(sampledParent).Should().Be(1.0);

        var notSampledParent = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test",
            ParentSampled = false
        };
        sampler(notSampledParent).Should().Be(0.0);

        var rootTransaction = new PluginTransactionSamplingContext
        {
            TransactionName = "test",
            Operation = "test"
        };
        sampler(rootTransaction).Should().Be(0.5);
    }

    #endregion
}
