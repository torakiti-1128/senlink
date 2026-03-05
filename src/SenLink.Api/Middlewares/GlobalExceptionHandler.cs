using Microsoft.AspNetCore.Diagnostics;
using SenLink.Api.Models;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// グローバル例外ハンドラー
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        // ロガー
        private readonly ILogger<GlobalExceptionHandler> _logger;

        // コンストラクタ
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 例外を処理し適切なHTTPレスポンスを返す
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception: {Message} at {Path}", exception.Message, httpContext.Request.Path);

            var (statusCode, message, errorType, details) = exception switch
            {
                SenLinkException ex => (ex.StatusCode, ex.Message, ex.ErrorType, ex.Errors),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "SERVER_ERROR", null)
            };

            // APIエラー形式でレスポンスを構築
            var response = new ApiErrorResponse
            {
                Success = false,
                Code = statusCode,
                Message = message,
                Operation = GetOperationName(httpContext),
                Error = new ErrorDetail
                {
                    Type = errorType,
                    Details = details
                }
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        /// <summary>
        /// HTTPコンテキストから操作名を取得するヘルパーメソッド
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetOperationName(HttpContext context)
        {
            // ルート値からアクション名を取得
            var action = context.Request.RouteValues["action"]?.ToString();
            return action ?? context.Request.Path.Value?.Trim('/').Replace("/", "_") ?? "unknown_operation";
        }
    }
}