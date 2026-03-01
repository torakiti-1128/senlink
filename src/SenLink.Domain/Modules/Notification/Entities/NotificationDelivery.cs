using SenLink.Domain.Common;
using SenLink.Domain.Modules.Notification.Enums;

namespace SenLink.Domain.Modules.Notification.Entities;

/// <summary>
/// 送達状態：各チャネル（メール、LINE等）への実際の送信ログとリトライ管理
/// </summary>
public class NotificationDelivery : BaseEntity
{
    // 通知ID (FK, NN)
    public long NotificationId { get; set; }

    // 受信者ID (NOFK, NN, accounts.id) ※検索最適化用の非正規化データ
    public long RecipientAccountId { get; set; }

    // 送信チャネル (SMALLINT, NN)
    public DeliveryChannel Channel { get; set; }

    // 送信ステータス (SMALLINT, NN, Default: 0)
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

    // 外部事業者のメッセージID (VARCHAR(128))
    public string? ProviderMessageId { get; set; }

    // 失敗種別 (VARCHAR(64))
    public string? ErrorType { get; set; }

    // エラー詳細 (TEXT)
    public string? ErrorMessage { get; set; }

    // 送信試行回数 (INT, NN, Default: 0)
    public int AttemptCount { get; set; } = 0;

    // 次回リトライ時刻 (TIMESTAMP)
    public DateTime? NextRetryAt { get; set; }

    // 通知とのリレーション
    public Notification Notification { get; set; } = null!;
}