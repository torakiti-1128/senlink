namespace SenLink.Api.Tests;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Theory]
    [InlineData(typeof(ForbiddenException), 403, "FORBIDDEN_ERROR")]
    [InlineData(typeof(Exception), 500, "SERVER_ERROR")]
    public async Task TryHandleAsync_ReturnsCorrectStatusAndFormat(Type exceptionType, int expectedCode, string expectedType)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = exceptionType == typeof(Exception) 
            ? new Exception("System Error") 
            : (Exception)Activator.CreateInstance(exceptionType)!;

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.Equal(expectedCode, context.Response.StatusCode);
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = new StreamReader(context.Response.Body).ReadToEnd();
        var response = JsonSerializer.Deserialize<ApiErrorResponse>(responseText);
        
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(expectedCode, response.Code);
        Assert.Equal(expectedType, response.Error?.Type);
    }
}