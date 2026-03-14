using MassTransit;
using SenLink.Domain.Modules.Audit.Contracts;
using System.Security.Claims;

namespace SenLink.Api.Middlewares;

/// <summary>
/// すべてのAPIアクセスを監査ログとして記録するミドルウェア
/// </summary>
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IPublishEndpoint publishEndpoint)
    {
        _logger.LogInformation("AuditLogMiddleware starting for {Method} {Path}", context.Request.Method, context.Request.Path);
        
        // 次の処理へ
        await _next(context);

        // 正常終了（2xx系）のみ記録
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            _logger.LogInformation("Publishing AuditLog event for {Method} {Path}", context.Request.Method, context.Request.Path);
            try
            {
                var actorId = GetCurrentUserId(context);
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var now = DateTime.UtcNow;

                var eventData = new AuditLogCreatedEvent(
                    actorId,
                    "API_ACCESS",
                    0,
                    context.Request.Method,
                    new Dictionary<string, object> { { "Path", context.Request.Path.Value ?? "" } },
                    new Dictionary<string, object> { { "StatusCode", context.Response.StatusCode } },
                    ipAddress,
                    now
                );

                await publishEndpoint.Publish(eventData);
                _logger.LogInformation("Successfully published AuditLog event.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish audit log event for {Path}.", context.Request.Path);
            }
        }
    }

    private long GetCurrentUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return long.TryParse(userIdClaim?.Value, out var id) ? id : 0;
    }
}