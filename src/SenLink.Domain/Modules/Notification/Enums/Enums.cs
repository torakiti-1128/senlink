namespace SenLink.Domain.Modules.Notification.Enums;

/// <summary>
/// 通知の種別
/// </summary>
public enum NotificationType : short
{
    // システム
    System = 0,

    // 申請
    Request = 1,

    // 求人
    Job = 2,

    // 活動
    Activity = 3,

    // 催促
    Reminder = 4,

    // レコメンド
    Recommendation = 5,

    // その他
    Other = 9
}

/// <summary>
/// 通知の既読ステータス
/// </summary>
public enum ReadStatus : short
{
    // 未読
    Unread = 0,

    // 既読
    Read = 1
}

/// <summary>
/// 通知の送信チャネル
/// </summary>
public enum DeliveryChannel : short
{
    // 通知センター
    InApp = 0,

    // メール
    Email = 1,

    // LINE
    Line = 2
}

/// <summary>
/// 通知の送信ステータス
/// </summary>
public enum DeliveryStatus : short
{
    // 送信前
    Pending = 0,

    // 送信済
    Sent = 1,

    // 失敗
    Failed = 2
}

/// <summary>
/// LINEの連携ステータス
/// </summary>
public enum LineLinkStatus : short
{
    // 未連携
    Unlinked = 0,

    // 連携済
    Linked = 1,

    // 解除
    UnlinkedByUser = 2
}