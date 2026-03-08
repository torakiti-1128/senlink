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
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

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

            // スコープを作成して IPublishEndpoint を取得する
            using (var scope = _serviceProvider.CreateScope())
            {
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                await PublishErrorLogEvent(httpContext, publishEndpoint, exception, cancellationToken);
            }

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

        private async Task PublishErrorLogEvent(HttpContext context, IPublishEndpoint publishEndpoint, Exception exception, CancellationToken ct)
        {
            try
            {
                var accountId = GetCurrentUserId(context);
                var request = context.Request;

                var requestParams = new RequestParams
                {
                    QueryString = request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
                    Headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
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

                await publishEndpoint.Publish(errorEvent, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish error log event to RabbitMQ");
            }
        }

        private long GetCurrentUserId(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdClaim?.Value, out var id) ? id : 0;
        }

        private string GetOperationName(HttpContext context)
        {
            var action = context.Request.RouteValues["action"]?.ToString();
            return action ?? context.Request.Path.Value?.Trim('/').Replace("/", "_") ?? "unknown_operation";
        }
    }
}
