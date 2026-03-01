namespace SenLink.Domain.Modules.Notification.Entities;

/// <summary>
/// 受信管理：ユーザーごとの通知チャネルごとのON/OFF設定
/// </summary>
public class NotificationPreference
{
    // アカウントID (PK, NOFK, NN, accounts.id)
    public long AccountId { get; set; }

    // 通知センター有効フラグ (BOOLEAN, NN, Default: true)
    public bool InAppEnabled { get; set; } = true;

    // メール有効フラグ (BOOLEAN, NN, Default: true)
    public bool EmailEnabled { get; set; } = true;

    // LINE有効フラグ (BOOLEAN, NN, Default: false)
    public bool LineEnabled { get; set; } = false;

    // 全停止フラグ (BOOLEAN, NN, Default: false)
    public bool MuteAll { get; set; } = false;

    // Entity共通のタイムスタンプ（独自FKの設計で BaseEntity を継承しないため手動定義）
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}