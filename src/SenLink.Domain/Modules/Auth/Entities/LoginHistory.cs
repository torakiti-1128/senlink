namespace SenLink.Domain.Modules.Auth.Entities;

/// <summary>
/// ログイン履歴：アクセス元情報の記録
/// </summary>
public class LoginHistory : BaseEntity
{
    // アカウントID (FK, NN)
    public long AccountId { get; set; }

    // IPアドレス (VARCHAR(45))
    public string? IpAddress { get; set; }

    // ブラウザ情報 (TEXT)
    public string? UserAgent { get; set; }

    // ステータス (SMALLINT) 0:失敗／1:成功
    public short Status { get; set; }
}