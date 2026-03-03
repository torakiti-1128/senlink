namespace SenLink.Api.Tests;

/// <summary>
/// 共通例外ハンドラーのレスポンステスト
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    // テストケース
    public static IEnumerable<object[]> GetExceptionTestCases()
    {
        yield return new object[] { new BadRequestException(), 400, "BAD_REQUEST_ERROR" };
        yield return new object[] { new UnauthorizedException(), 401, "UNAUTHORIZED_ERROR" };
        yield return new object[] { new ForbiddenException(), 403, "FORBIDDEN_ERROR" };
        yield return new object[] { new NotFoundException(), 404, "NOT_FOUND_ERROR" };
        yield return new object[] { new ConflictException(), 409, "CONFLICT_ERROR" };
        yield return new object[] { new ValidationException(new List<ValidationErrorDetail>()), 422, "VALIDATION_ERROR" };
        yield return new object[] { new TooManyRequestsException(), 429, "TOO_MANY_REQUESTS_ERROR" };
        yield return new object[] { new Exception("System Error"), 500, "SERVER_ERROR" };
    }

    [Theory]
    [MemberData(nameof(GetExceptionTestCases))] 
    public async Task TryHandleAsync_ReturnsCorrectStatusAndFormat(Exception exception, int expectedCode, string expectedType)
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