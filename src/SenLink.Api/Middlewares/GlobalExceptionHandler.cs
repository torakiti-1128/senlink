using Microsoft.AspNetCore.Diagnostics;
using SenLink.Api.Models;
using System.Security.Claims;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// グローバル例外ハンドラー
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        private string GetOperationName(HttpContext context)
        {
            var action = context.Request.RouteValues["action"]?.ToString();
            return action ?? context.Request.Path.Value?.Trim('/').Replace("/", "_") ?? "unknown_operation";
        }
    }
}
