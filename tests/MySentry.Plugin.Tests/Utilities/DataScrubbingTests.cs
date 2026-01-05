using FluentAssertions;
using MySentry.Plugin.Configuration;
using MySentry.Plugin.Utilities;
using Xunit;

namespace MySentry.Plugin.Tests.Utilities;

/// <summary>
/// Tests for DataScrubber and DataScrubbingOptions.
/// </summary>
public class DataScrubbingTests
{
    #region DataScrubbingOptions Tests

    [Fact]
    public void DataScrubbingOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new DataScrubbingOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.ReplacementText.Should().Be("[Filtered]");
        options.ScrubRequestBodies.Should().BeTrue();
        options.ScrubQueryStrings.Should().BeTrue();
        options.ScrubCookies.Should().BeTrue();
    }

    [Fact]
    public void DataScrubbingOptions_DefaultSensitiveFields_ContainsExpectedValues()
    {
        // Act
        var options = new DataScrubbingOptions();

        // Assert
        options.SensitiveFields.Should().Contain("password");
        options.SensitiveFields.Should().Contain("token");
        options.SensitiveFields.Should().Contain("apikey");
        options.SensitiveFields.Should().Contain("secret");
        options.SensitiveFields.Should().Contain("credentials");
        options.SensitiveFields.Should().Contain("authorization");
    }

    [Fact]
    public void DataScrubbingOptions_DefaultSensitivePatterns_ContainsExpectedPatterns()
    {
        // Act
        var options = new DataScrubbingOptions();

        // Assert
        options.SensitivePatterns.Should().HaveCountGreaterThan(0);
        // Should have credit card pattern
        options.SensitivePatterns.Should().Contain(p => p.Contains("\\d{4}"));
    }

    [Fact]
    public void DataScrubbingOptions_DefaultSensitiveHeaders_ContainsExpectedValues()
    {
        // Act
        var options = new DataScrubbingOptions();

        // Assert
        options.SensitiveHeaders.Should().Contain("Authorization");
        options.SensitiveHeaders.Should().Contain("Cookie");
        options.SensitiveHeaders.Should().Contain("X-API-Key");
    }

    [Fact]
    public void DataScrubbingOptions_CanAddCustomField()
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        options.SensitiveFields.Add("my_custom_field");

        // Assert
        options.SensitiveFields.Should().Contain("my_custom_field");
    }

    [Fact]
    public void DataScrubbingOptions_CanAddCustomPattern()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var passportPattern = @"\b[A-Z]{2}\d{9}\b";

        // Act
        options.SensitivePatterns.Add(passportPattern);

        // Assert
        options.SensitivePatterns.Should().Contain(passportPattern);
    }

    #endregion

    #region DataScrubber.IsSensitiveField Tests

    [Theory]
    [InlineData("password", true)]
    [InlineData("Password", true)]
    [InlineData("PASSWORD", true)]
    [InlineData("user_password", true)]
    [InlineData("passwordHash", true)]
    [InlineData("token", true)]
    [InlineData("access_token", true)]
    [InlineData("apikey", true)]
    [InlineData("api_key", true)]
    [InlineData("secret", true)]
    [InlineData("client_secret", true)]
    [InlineData("username", false)]
    [InlineData("email", false)]
    [InlineData("name", false)]
    public void IsSensitiveField_WithDefaultOptions_ReturnsExpectedResult(string fieldName, bool expected)
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        var result = DataScrubber.IsSensitiveField(fieldName, options);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsSensitiveField_WhenDisabled_StillDetectsSensitiveFields()
    {
        // Arrange
        // Note: IsSensitiveField doesn't check Enabled flag - it only checks if the field matches
        // The Enabled flag is checked at the scrubbing level, not detection level
        var options = new DataScrubbingOptions { Enabled = false };

        // Act
        var result = DataScrubber.IsSensitiveField("password", options);

        // Assert - IsSensitiveField still identifies sensitive fields regardless of Enabled flag
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSensitiveField_WithCustomField_DetectsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        options.SensitiveFields.Add("ssn");

        // Act
        var result = DataScrubber.IsSensitiveField("user_ssn", options);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSensitiveField_NullFieldName_ReturnsFalse()
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        var result = DataScrubber.IsSensitiveField(null!, options);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DataScrubber.ScrubString Tests

    [Fact]
    public void ScrubString_WithCreditCardNumber_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var input = "My card is 4111-1111-1111-1111";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().NotContain("4111-1111-1111-1111");
        result.Should().Contain("[Filtered]");
    }

    [Fact]
    public void ScrubString_WithCreditCardNoSpaces_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var input = "Card: 4111111111111111";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().NotContain("4111111111111111");
        result.Should().Contain("[Filtered]");
    }

    [Fact]
    public void ScrubString_WithSSN_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var input = "SSN: 123-45-6789";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().NotContain("123-45-6789");
        result.Should().Contain("[Filtered]");
    }

    [Fact]
    public void ScrubString_WithJWT_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var input = $"Token: {jwt}";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().NotContain(jwt);
        result.Should().Contain("[Filtered]");
    }

    [Fact]
    public void ScrubString_WithNullInput_ReturnsNull()
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        var result = DataScrubber.ScrubString(null, options);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ScrubString_WhenDisabled_ReturnsOriginal()
    {
        // Arrange
        var options = new DataScrubbingOptions { Enabled = false };
        var input = "Card: 4111-1111-1111-1111";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void ScrubString_WithCustomReplacementText_UsesIt()
    {
        // Arrange
        var options = new DataScrubbingOptions { ReplacementText = "[REDACTED]" };
        var input = "Card: 4111-1111-1111-1111";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().Contain("[REDACTED]");
        result.Should().NotContain("[Filtered]");
    }

    [Fact]
    public void ScrubString_WithCustomPattern_AppliesIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        options.SensitivePatterns.Add(@"\bPASS\d{4}\b"); // Custom pattern like PASS1234

        var input = "Access code: PASS1234";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().NotContain("PASS1234");
        result.Should().Contain("[Filtered]");
    }

    [Fact]
    public void ScrubString_NoSensitiveData_ReturnsOriginal()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var input = "Hello, this is a normal message.";

        // Act
        var result = DataScrubber.ScrubString(input, options);

        // Assert
        result.Should().Be(input);
    }

    #endregion

    #region DataScrubber.ScrubDictionary Tests

    [Fact]
    public void ScrubDictionary_WithSensitiveKeys_ScrubsValues()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var dict = new Dictionary<string, string>
        {
            ["username"] = "john",
            ["password"] = "secret123",
            ["api_key"] = "my-api-key"
        };

        // Act
        var result = DataScrubber.ScrubDictionary(dict, options);

        // Assert
        result.Should().NotBeNull();
        result!["username"].Should().Be("john");
        result["password"].Should().Be("[Filtered]");
        result["api_key"].Should().Be("[Filtered]");
    }

    [Fact]
    public void ScrubDictionary_WithNullInput_ReturnsEmptyDictionary()
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        var result = DataScrubber.ScrubDictionary<string>(null, options);

        // Assert - Returns empty dictionary for null input, not null
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ScrubDictionary_WhenDisabled_ReturnsOriginal()
    {
        // Arrange
        var options = new DataScrubbingOptions { Enabled = false };
        var dict = new Dictionary<string, string>
        {
            ["password"] = "secret123"
        };

        // Act
        var result = DataScrubber.ScrubDictionary(dict, options);

        // Assert
        result!["password"].Should().Be("secret123");
    }

    [Fact]
    public void ScrubDictionary_WithObjectValues_ScrubsCorrectly()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var dict = new Dictionary<string, object>
        {
            ["user"] = "john",
            ["token"] = "abc123"
        };

        // Act
        var result = DataScrubber.ScrubDictionary(dict, options);

        // Assert
        result!["user"].Should().Be("john");
        result["token"].Should().Be("[Filtered]");
    }

    #endregion

    #region DataScrubber.ScrubHeaders Tests

    [Fact]
    public void ScrubHeaders_WithAuthorizationHeader_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Authorization"] = "Bearer eyJhbGciOiJIUzI1..."
        };

        // Act
        var result = DataScrubber.ScrubHeaders(headers, options);

        // Assert
        result!["Content-Type"].Should().Be("application/json");
        result["Authorization"].Should().Be("[Filtered]");
    }

    [Fact]
    public void ScrubHeaders_WithCookieHeader_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var headers = new Dictionary<string, string>
        {
            ["Cookie"] = "session=abc123; auth=xyz789"
        };

        // Act
        var result = DataScrubber.ScrubHeaders(headers, options);

        // Assert
        result!["Cookie"].Should().Be("[Filtered]");
    }

    [Fact]
    public void ScrubHeaders_WithXApiKey_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var headers = new Dictionary<string, string>
        {
            ["X-Api-Key"] = "my-secret-api-key"
        };

        // Act
        var result = DataScrubber.ScrubHeaders(headers, options);

        // Assert
        result!["X-Api-Key"].Should().Be("[Filtered]");
    }

    [Fact]
    public void ScrubHeaders_CaseInsensitive_Works()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var headers = new Dictionary<string, string>
        {
            ["authorization"] = "Bearer token",
            ["COOKIE"] = "session=abc"
        };

        // Act
        var result = DataScrubber.ScrubHeaders(headers, options);

        // Assert
        result!["authorization"].Should().Be("[Filtered]");
        result["COOKIE"].Should().Be("[Filtered]");
    }

    [Fact]
    public void ScrubHeaders_WithCustomHeader_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        options.SensitiveHeaders.Add("X-Custom-Secret");
        var headers = new Dictionary<string, string>
        {
            ["X-Custom-Secret"] = "my-secret"
        };

        // Act
        var result = DataScrubber.ScrubHeaders(headers, options);

        // Assert
        result!["X-Custom-Secret"].Should().Be("[Filtered]");
    }

    #endregion

    #region DataScrubber.ScrubQueryString Tests

    [Fact]
    public void ScrubQueryString_WithPasswordParam_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var queryString = "?user=john&password=secret123";

        // Act
        var result = DataScrubber.ScrubQueryString(queryString, options);

        // Assert
        result.Should().Contain("user=john");
        result.Should().NotContain("secret123");
        result.Should().Contain("password=[Filtered]");
    }

    [Fact]
    public void ScrubQueryString_WithTokenParam_ScrubsIt()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var queryString = "callback_url=http://example.com&access_token=abc123";

        // Act
        var result = DataScrubber.ScrubQueryString(queryString, options);

        // Assert
        result.Should().Contain("callback_url=http://example.com");
        result.Should().NotContain("abc123");
        result.Should().Contain("access_token=[Filtered]");
    }

    [Fact]
    public void ScrubQueryString_WhenDisabled_ReturnsOriginal()
    {
        // Arrange
        var options = new DataScrubbingOptions { Enabled = false };
        var queryString = "?password=secret123";

        // Act
        var result = DataScrubber.ScrubQueryString(queryString, options);

        // Assert
        result.Should().Be(queryString);
    }

    [Fact]
    public void ScrubQueryString_WithScrubQueryStringsDisabled_ReturnsOriginal()
    {
        // Arrange
        var options = new DataScrubbingOptions { ScrubQueryStrings = false };
        var queryString = "?password=secret123";

        // Act
        var result = DataScrubber.ScrubQueryString(queryString, options);

        // Assert
        result.Should().Be(queryString);
    }

    [Fact]
    public void ScrubQueryString_NullInput_ReturnsNull()
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        var result = DataScrubber.ScrubQueryString(null, options);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ScrubQueryString_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var options = new DataScrubbingOptions();

        // Act
        var result = DataScrubber.ScrubQueryString(string.Empty, options);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ScrubQueryString_MultipleParams_ScrubsOnlySensitive()
    {
        // Arrange
        var options = new DataScrubbingOptions();
        var queryString = "name=John&email=john@example.com&api_key=secret&page=1";

        // Act
        var result = DataScrubber.ScrubQueryString(queryString, options);

        // Assert
        result.Should().Contain("name=John");
        result.Should().Contain("email=john@example.com");
        result.Should().Contain("page=1");
        result.Should().Contain("api_key=[Filtered]");
        result.Should().NotContain("secret");
    }

    #endregion
}
