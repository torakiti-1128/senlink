using MassTransit;
using Microsoft.AspNetCore.Diagnostics;
using SenLink.Api.Models;
using SenLink.Domain.Modules.Audit.Contracts;
using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Enums;
using System.Security.Claims;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// グローバル例外ハンドラー
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        // ロガー
        private readonly ILogger<GlobalExceptionHandler> _logger;
        
        // メッセージ送信エンドポイント
        private readonly IPublishEndpoint _publishEndpoint;

        // コンストラクタ
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        /// <summary>
        /// 例外を処理し適切なHTTPレスポンスを返す
        /// </summary>
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

            // ErrorLogを作成してRabbitMQにパブリッシュ（非同期ログ記録）
            await PublishErrorLogEvent(httpContext, exception, cancellationToken);

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
        /// エラー情報をRabbitMQに送信する
        /// </summary>
        private async Task PublishErrorLogEvent(HttpContext context, Exception exception, CancellationToken ct)
        {
            try
            {
                var accountId = GetCurrentUserId(context);
                var request = context.Request;

                var requestParams = new RequestParams
                {
                    QueryString = request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
                    Headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                    // ToDo：Bodyの読み取りはバッファリング設定が必要なため、現時点では一旦スキップ
                    Body = null 
                };

                var errorEvent = new ErrorLogCreatedEvent(
                    ServiceName: "SenLink.Api",
                    Severity: ErrorSeverity.Error,
                    Message: exception.Message,
                    StackTrace: exception.StackTrace,
                    RequestUrl: $"{request.Method} {request.Path}",
                    RequestParams: requestParams,
                    AccountId: accountId > 0 ? accountId : null
                );

                await _publishEndpoint.Publish(errorEvent, ct);
            }
            catch (Exception ex)
            {
                // ログ記録自体の失敗は、ローカルロガーにのみ記録してアプリのレスポンスを妨げないようにする
                _logger.LogWarning(ex, "Failed to publish error log event to RabbitMQ");
            }
        }

        /// <summary>
        /// 現在のユーザーIDを取得する
        /// </summary>
        private long GetCurrentUserId(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdClaim?.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// HTTPコンテキストから操作名を取得するヘルパーメソッド
        /// </summary>
        private string GetOperationName(HttpContext context)
        {
            var action = context.Request.RouteValues["action"]?.ToString();
            return action ?? context.Request.Path.Value?.Trim('/').Replace("/", "_") ?? "unknown_operation";
        }
    }
}
 public partial class Program { }
