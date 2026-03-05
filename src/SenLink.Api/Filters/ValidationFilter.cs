using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace SenLink.Api.Filters
{
    /// <summary>
    /// モデルのバリデーションをチェックし、エラーがあれば統一された形式で400 Bad Requestを返すフィルター
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // モデル状態が無効（バリデーションエラーがある）場合
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    success = false,
                    data = errors, // どの項目がどうエラーなのかをDataに含める
                    errorMessage = "One or more validation errors occurred."
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}