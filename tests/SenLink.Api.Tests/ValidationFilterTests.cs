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
        
        // 型安全に値を確認するためにリフレクションを使用
        var response = result.Value;
        Assert.NotNull(response);
        
        var successProp = response.GetType().GetProperty("success");
        var errorMessageProp = response.GetType().GetProperty("errorMessage");
        
        Assert.NotNull(successProp);
        Assert.NotNull(errorMessageProp);
        
        Assert.False((bool?)successProp.GetValue(response));
        Assert.Equal("One or more validation errors occurred.", errorMessageProp.GetValue(response));
    }
}
