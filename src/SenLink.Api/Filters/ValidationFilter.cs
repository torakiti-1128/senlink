using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SenLink.Api.Models;

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
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => new ValidationErrorDetail
                    {
                        Field = kvp.Key,
                        Reason = e.ErrorMessage
                    }))
                    .ToList();

                var response = new ApiErrorResponse
                {
                    Success = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "One or more validation errors occurred.",
                    Operation = GetOperationName(context.HttpContext),
                    Error = new ErrorDetail
                    {
                        Type = "VALIDATION_ERROR",
                        Details = errors
                    }
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        private string GetOperationName(HttpContext context)
        {
            var action = context.Request.RouteValues["action"]?.ToString();
            return action ?? context.Request.Path.Value?.Trim('/').Replace("/", "_") ?? "unknown_operation";
        }
    }
}