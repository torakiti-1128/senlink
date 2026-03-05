using SenLink.Service.Modules.Maintenance.Interfeces;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// 学外IPからのアクセスを制限するミドルウェア
    /// </summary>
    public class CampusIpRestriction
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CampusIpRestriction> _logger;

        public CampusIpRestriction(RequestDelegate next, ILogger<CampusIpRestriction> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// リクエストのIPアドレスをチェックし、許可されていない場合はアクセスを拒否する
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        /// <exception cref="ForbiddenException"></exception>
        public async Task InvokeAsync(HttpContext context, ISystemSettingProvider provider)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            
            // サービス層のプロバイダーから設定値を取得
            var allowedIpsString = provider.GetValue("CampusIps");

            if (!string.IsNullOrWhiteSpace(allowedIpsString) && !string.IsNullOrEmpty(clientIp))
            {
                var allowedIps = allowedIpsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (!allowedIps.Contains(clientIp))
                {
                    _logger.LogWarning("Unauthorized access attempt from IP: {ClientIp}", clientIp);
                    throw new ForbiddenException("Not permitted to access from this IP address.");
                }
            }

            _logger.LogInformation("Access granted for IP: {ClientIp}", clientIp);

            await _next(context);
        }
    }
}