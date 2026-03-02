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
    [InlineData(typeof(BadRequestException), 400, "BAD_REQUEST_ERROR")]
    [InlineData(typeof(UnauthorizedException), 401, "UNAUTHORIZED_ERROR")]
    [InlineData(typeof(ForbiddenException), 403, "FORBIDDEN_ERROR")]
    [InlineData(typeof(NotFoundException), 404, "NOT_FOUND_ERROR")]
    [InlineData(typeof(ConflictException), 409, "CONFLICT_ERROR")]
    [InlineData(typeof(ValidationException), 422, "VALIDATION_ERROR")]
    [InlineData(typeof(TooManyRequestsException), 429, "TOO_MANY_REQUESTS_ERROR")]
    [InlineData(typeof(Exception), 500, "SERVER_ERROR")]
    public async Task TryHandleAsync_ReturnsCorrectStatusAndFormat(Type exceptionType, int expectedCode, string expectedType)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // インスタンス化
        Exception exception = exceptionType == typeof(ForbiddenException)
            ? new ForbiddenException()
            : new Exception("System Error");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedCode, context.Response.StatusCode);
        
        responseStream.Seek(0, SeekOrigin.Begin);
        
        var response = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(
            responseStream, 
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal(expectedCode, response.Code);
        Assert.Equal(expectedType, response.Error?.Type);
    }
}