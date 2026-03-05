global using SenLink.Api.Middlewares;
global using SenLink.Api.Models;

namespace SenLink.Api.Tests;

public class SuccessResponseFilterTests
{
    private readonly SuccessResponseFilter _filter;

    public SuccessResponseFilterTests()
    {
        _filter = new SuccessResponseFilter();
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldWrapObjectResult_WhenStatusCodeIs2xx()
    {
        // Arrange
        // コントローラーが return Ok(new { Name = "Taro" }); を返した状況を再現
        var originalData = new { Name = "Taro" };
        var objectResult = new ObjectResult(originalData) { StatusCode = 200 };
        var context = CreateContext(objectResult);
        var nextCalled = false;
        ResultExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ResultExecutedContext(context, new List<IFilterMetadata>(), context.Result, new object()));
        };

        // Act
        await _filter.OnResultExecutionAsync(context, next);

        // Assert
        Assert.True(nextCalled); // 次の処理が呼ばれたか
        Assert.IsType<ApiResponse<object>>(objectResult.Value); // ApiResponse<object> に差し替えられているか
        
        var wrappedResponse = (ApiResponse<object>)objectResult.Value!;
        Assert.True(wrappedResponse.Success);
        Assert.Equal(200, wrappedResponse.Code);
        Assert.Equal("OK", wrappedResponse.Message);
        Assert.Equal(originalData, wrappedResponse.Data); // 元のデータが Data プロパティに入っているか
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldWrapStatusCodeResult_WhenStatusCodeIs2xx()
    {
        // Arrange
        // コントローラーが return NoContent(); (204) などを返した状況を再現
        var statusCodeResult = new StatusCodeResult(204);
        var context = CreateContext(statusCodeResult);
        ResultExecutionDelegate next = () => Task.FromResult(new ResultExecutedContext(context, new List<IFilterMetadata>(), context.Result, new object()));

        // Act
        await _filter.OnResultExecutionAsync(context, next);

        // Assert
        // 結果が ObjectResult に差し替えられ、中身がデータなしの ApiResponse になっているか
        var result = Assert.IsType<ObjectResult>(context.Result);
        var wrappedResponse = Assert.IsType<ApiResponse>(result.Value);
        
        Assert.True(wrappedResponse.Success);
        Assert.Equal(204, wrappedResponse.Code);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldNotDoubleWrap_WhenAlreadyApiResponse()
    {
        // Arrange
        // 開発者がすでに return new ApiResponse<string> { ... } と明示的に返した状況を再現
        var alreadyWrappedData = new ApiResponse<string> { Success = true, Code = 200, Data = "Test" };
        var objectResult = new ObjectResult(alreadyWrappedData) { StatusCode = 200 };
        var context = CreateContext(objectResult);
        ResultExecutionDelegate next = () => Task.FromResult(new ResultExecutedContext(context, new List<IFilterMetadata>(), context.Result, new object()));

        // Act
        await _filter.OnResultExecutionAsync(context, next);

        // Assert
        // 二重に ApiResponse に包まれておらず、元の型のまま維持されているか
        Assert.Same(alreadyWrappedData, objectResult.Value); 
    }

    // テスト用の ResultExecutingContext を生成するヘルパーメソッド
    private static ResultExecutingContext CreateContext(IActionResult result)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        return new ResultExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            result,
            new object()
        );
    }
}