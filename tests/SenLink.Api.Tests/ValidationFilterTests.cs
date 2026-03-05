using SenLink.Api.Filters;

namespace SenLink.Api.Tests.Filters;

public class ValidationFilterTests
{
    [Fact]
    public void OnActionExecuting_バリデーションエラーがある場合_400を返すこと()
    {
        // Arrange
        var filter = new ValidationFilter();
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );
        // 意図的にエラーを注入
        actionContext.ModelState.AddModelError("Name", "Name is required");

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new Mock<Controller>().Object
        );

        // Act
        filter.OnActionExecuting(context);

        // Assert
        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        // JSONの中身（success=falseなど）が期待通りか確認
        dynamic? response = result.Value;
        Assert.False(response?.success);
        Assert.Equal("One or more validation errors occurred.", response?.errorMessage);
    }
}