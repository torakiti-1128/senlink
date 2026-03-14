using Microsoft.Extensions.Logging;
using SenLink.Service.Common.Interfaces;

namespace SenLink.Service.Common.Services;

/// <summary>
/// 開発環境向けのメール送信サービス実装（ログ出力のみ）
/// </summary>
public class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        logger.LogInformation("================================================");
        logger.LogInformation("【EMAIL SEND SIMULATION】");
        logger.LogInformation("To: {To}", to);
        logger.LogInformation("Subject: {Subject}", subject);
        logger.LogInformation("Body: {Body}", body);
        logger.LogInformation("================================================");
        
        return Task.CompletedTask;
    }
}
