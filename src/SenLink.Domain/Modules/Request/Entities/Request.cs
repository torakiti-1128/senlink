using SenLink.Domain.Common;
using SenLink.Domain.Modules.Request.Enums;

namespace SenLink.Domain.Modules.Request.Entities;

/// <summary>
/// 申請：学生の各種申請と教員のステート管理
/// </summary>
public class Request : BaseEntity
{
    // 申請者ID (NOFK, NN, accounts.id)
    public long RequesterAccountId { get; set; }

    // 承認/差し戻し担当ID (NOFK, accounts.id)
    public long? ReviewerAccountId { get; set; }

    // 種別 (SMALLINT, NN)
    public RequestType Type { get; set; }

    // ステータス (SMALLINT, NN, Default: 0)
    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    // 一覧表示用タイトル (VARCHAR(255), NN)
    public string Title { get; set; } = null!;

    // 種別ごとの入力内容 (JSONB, NN)
    public RequestPayload Payload { get; set; } = null!;

    // 申請送信日時 (TIMESTAMP)
    public DateTime? SubmittedAt { get; set; }

    // 承認/差し戻し確定日時 (TIMESTAMP)
    public DateTime? ResolvedAt { get; set; }

    // 申請コメントとのリレーション
    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();

    // 申請添付資料とのリレーション
    public ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
}

// 申請の種別ごとの入力内容を柔軟に管理する
public class RequestPayload
{
    // 面接予約時の希望日時や、書類提出時のメモなど
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}