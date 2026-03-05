using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SenLink.Api.Models;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// コントローラーのアクションが返す成功レスポンスを ApiResponse 形式にラップするフィルター
    /// </summary>
    public class SuccessResponseFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // コントローラーが正常なデータ（ObjectResult）を返した場合のみ処理
            if (context.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

                // 200番台（成功）かつ、まだ ApiResponse でラップされていない場合
                if (statusCode >= 200 && statusCode < 300 && objectResult.Value is not ApiResponse)
                {
                    var wrapper = new ApiResponse<object>
                    {
                        Success = true,
                        Code = statusCode,
                        Message = "OK",
                        Data = objectResult.Value,
                        Operation = GetOperationName(context.HttpContext)
                    };

                    // ラップしたオブジェクトに差し替える
                    objectResult.Value = wrapper;
                }
            }
            // データなしの成功（204 NoContent や 200 OKのみ）を返す場合
            else if (context.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300)
            {
                var wrapper = new ApiResponse
                {
                    Success = true,
                    Code = statusCodeResult.StatusCode,
                    Message = "OK",
                    Operation = GetOperationName(context.HttpContext)
                };

                // JSON として返すようにする
                context.Result = new ObjectResult(wrapper) { StatusCode = statusCodeResult.StatusCode };
            }

            // 次の処理（実際のレスポンス書き込み）へ進む
            await next();
        }

        private string GetOperationName(HttpContext context)
        {
            var action = context.Request.RouteValues["action"]?.ToString();
            return action ?? context.Request.Path.Value?.Trim('/').Replace("/", "_") ?? "unknown_operation";
        }
    }
}