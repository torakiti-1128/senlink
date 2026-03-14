namespace SenLink.Service.Common.Interfaces;

/// <summary>
/// メール送信サービスのインターフェース
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// メールを送信する
    /// </summary>
    /// <param name="to">宛先</param>
    /// <param name="subject">件名</param>
    /// <param name="body">本文</param>
    Task SendEmailAsync(string to, string subject, string body);
}
