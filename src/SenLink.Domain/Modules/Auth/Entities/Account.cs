using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Auth.Entities;

/// <summary>
/// アカウント：認証基盤、ロール管理
/// </summary>
public class Account : BaseEntity
{
    // メールアドレス (UQ, NN)
    public string Email { get; set; } = string.Empty;

    // パスワード（Hash, NN)
    public string Password { get; set; } = string.Empty;

    // ロール (NN)
    public AccountRole Role { get; set; }

    // 有効フラグ (NN)
    public bool IsActive { get; set; } = true;

    // 論理削除
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// パスワードの設定
    /// </summary>
    /// <param name="rawPassword">平文のパスワード</param>
    public void SetPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword)) throw new ArgumentException("Password cannot be empty.");
        Password = BCrypt.Net.BCrypt.HashPassword(rawPassword);
    }

    /// <summary>
    /// パスワードの検証
    /// </summary>
    /// <param name="rawPassword">平文のパスワード</param>
    /// <returns>パスワードが一致する場合は true、それ以外は false</returns>
    public bool VerifyPassword(string rawPassword)
    {
        return BCrypt.Net.BCrypt.Verify(rawPassword, Password);
    }
}