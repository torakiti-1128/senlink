using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Auth.Entities;

/// <summary>
/// アカウント：認証基盤、ロール管理
/// </summary>
public class Account : BaseEntity
{
    // メールアドレス (UQ, NN)
    public string Email { get; set; } = string.Empty;

    // パスワード（Hash） (NN)
    public string Password { get; set; } = string.Empty;

    // ロール (NN) 0:学生／1:教員／2:管理者
    public short Role { get; set; }

    // 有効フラグ (NN)
    public bool IsActive { get; set; } = true;

    // 論理削除
    public DateTime? DeletedAt { get; set; }
}