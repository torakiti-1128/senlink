using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Auth.Entities;

/// <summary>
/// ワンタイムパスワード：新規登録、パスワード再設定用
/// </summary>
public class OneTimePassword : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public string Purpose { get; set; } = string.Empty; // "Register", "PasswordReset"
}
