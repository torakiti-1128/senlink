using System.Text.Json;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SenLink.Api.Middlewares;
using SenLink.Api.Models;
using SenLink.Domain.Modules.Audit.Contracts;
using Xunit;

namespace SenLink.Api.Tests;

/// <summary>
/// 共通例外ハンドラーのテスト
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        // IServiceProvider.CreateScope() のモック設定
        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
        
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IPublishEndpoint)))
            .Returns(_publishEndpointMock.Object);

        _handler = new GlobalExceptionHandler(_loggerMock.Object, _serviceProviderMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldPublishErrorLogCreatedEvent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/test-path";
        
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new Exception("Test error message");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<ErrorLogCreatedEvent>(e => 
                    e.Message == "Test error message" && 
                    e.RequestUrl == "GET /test-path" &&
                    e.ServiceName == "SenLink.Api"),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    public static IEnumerable<object[]> GetExceptionTestCases()
    {
        yield return new object[] { new BadRequestException("The request is invalid."), 400, "BAD_REQUEST_ERROR" };
        yield return new object[] { new Exception("System Error"), 500, "SERVER_ERROR" };
    }

    [Theory]
    [MemberData(nameof(GetExceptionTestCases))] 
    public async Task TryHandleAsync_ReturnsCorrectStatusAndFormat(Exception exception, int expectedCode, string expectedErrorType)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedCode, context.Response.StatusCode);

        // レスポンスボディの形式を検証
        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream);
        var responseBody = await reader.ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Equal(expectedCode, apiResponse.Code);
        Assert.Equal(expectedErrorType, apiResponse.Error.Type);
    }
}
