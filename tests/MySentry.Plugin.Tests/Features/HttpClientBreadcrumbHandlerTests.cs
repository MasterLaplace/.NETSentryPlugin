using FluentAssertions;
using Moq;
using Moq.Protected;
using MySentry.Plugin.Features.Breadcrumbs;
using System.Net;
using Xunit;

namespace MySentry.Plugin.Tests.Features;

/// <summary>
/// Tests for HttpClientBreadcrumbHandler.
/// </summary>
public class HttpClientBreadcrumbHandlerTests
{
    #region Constructor Tests

    [Fact]
    public void HttpClientBreadcrumbHandler_CanBeInstantiated()
    {
        // Act
        var handler = new HttpClientBreadcrumbHandler();

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void HttpClientBreadcrumbHandler_WithInnerHandler_CanBeInstantiated()
    {
        // Arrange
        var innerHandler = new Mock<HttpMessageHandler>();

        // Act
        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = innerHandler.Object
        };

        // Assert
        handler.Should().NotBeNull();
        handler.InnerHandler.Should().Be(innerHandler.Object);
    }

    #endregion

    #region SendAsync Tests

    [Fact]
    public async Task SendAsync_SuccessfulRequest_CompletesSuccessfully()
    {
        // Arrange
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_FailedRequest_PropagatesException()
    {
        // Arrange
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetAsync("https://api.example.com/test"));
    }

    [Fact]
    public async Task SendAsync_DifferentHttpMethods_Works()
    {
        // Arrange
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);

        // Act - POST
        var postResponse = await client.PostAsync(
            "https://api.example.com/resource",
            new StringContent("{}"));

        // Assert
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task SendAsync_CancellationToken_Works()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (request, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);

        // Act - Cancel immediately
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetAsync("https://api.example.com/test", cts.Token));
    }

    [Fact]
    public async Task SendAsync_MultipleRequests_AllComplete()
    {
        // Arrange
        var callCount = 0;
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                Interlocked.Increment(ref callCount);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);

        // Act - Send multiple requests
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.GetAsync("https://api.example.com/test"))
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        callCount.Should().Be(5);
        tasks.Should().AllSatisfy(t => t.Result.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task SendAsync_WithHeaders_PreservesHeaders()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("X-Custom-Header", "test-value");

        // Act
        await client.GetAsync("https://api.example.com/test");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.GetValues("X-Custom-Header").Should().Contain("test-value");
    }

    [Fact]
    public async Task SendAsync_DifferentStatusCodes_ReturnsCorrectStatus()
    {
        // Arrange
        var statusCodes = new[] 
        { 
            HttpStatusCode.OK, 
            HttpStatusCode.Created, 
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError 
        };
        
        var statusIndex = 0;
        var mockInnerHandler = new Mock<HttpMessageHandler>();
        mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(statusCodes[statusIndex++]));

        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = mockInnerHandler.Object
        };

        var client = new HttpClient(handler);

        // Act & Assert
        for (var i = 0; i < statusCodes.Length; i++)
        {
            statusIndex = i;
            var response = await client.GetAsync("https://api.example.com/test");
            response.StatusCode.Should().Be(statusCodes[i]);
        }
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var handler = new HttpClientBreadcrumbHandler
        {
            InnerHandler = new HttpClientHandler()
        };

        // Act & Assert - Should not throw
        handler.Dispose();
        handler.Dispose();
    }

    #endregion
}
