using SenLink.Api.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;
using System.Collections.Generic;

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
        
        // 新しい ApiErrorResponse 型として検証
        var response = Assert.IsType<ApiErrorResponse>(result.Value);
        
        Assert.False(response.Success);
        Assert.Equal(400, response.Code);
        Assert.Equal("One or more validation errors occurred.", response.Message);
        Assert.NotNull(response.Error);
        Assert.Equal("VALIDATION_ERROR", response.Error.Type);
        
        var details = Assert.IsType<List<ValidationErrorDetail>>(response.Error.Details);
        var error = Assert.Single(details);
        Assert.Equal("Name", error.Field);
        Assert.Equal("Name is required", error.Reason);
    }
}
