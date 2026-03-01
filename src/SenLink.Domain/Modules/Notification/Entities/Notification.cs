using SenLink.Domain.Common;
using SenLink.Domain.Modules.Notification.Enums;

namespace SenLink.Domain.Modules.Notification.Entities;

/// <summary>
/// 通知：ユーザーへのお知らせ内容本体
/// </summary>
public class Notification : BaseEntity
{
    // 受信者ID (NOFK, NN, accounts.id) ※他モジュールへの参照のためNOFKとします
    public long RecipientAccountId { get; set; }

    // タイトル (VARCHAR(255), NN)
    public string Title { get; set; } = null!;

    // 本文 (TEXT)
    public string? Body { get; set; }

    // 遷移URL (VARCHAR(255))
    public string? LinkUrl { get; set; }

    // 種別 (SMALLINT, NN)
    public NotificationType Type { get; set; }

    // 既読ステータス (SMALLINT, NN, Default: 0)
    public ReadStatus ReadStatus { get; set; } = ReadStatus.Unread;

    // 送達状態とのリレーション
    public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
}