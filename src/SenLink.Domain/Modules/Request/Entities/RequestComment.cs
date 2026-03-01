using SenLink.Domain.Common;
using SenLink.Domain.Modules.Request.Enums;

namespace SenLink.Domain.Modules.Request.Entities;

/// <summary>
/// コメント：申請に対するやり取りやステータス変更時のメモ
/// </summary>
public class RequestComment : BaseEntity
{
    // 申請ID (FK, NN)
    public long RequestId { get; set; }

    // 投稿者ID (NOFK, NN, accounts.id)
    public long AuthorAccountId { get; set; }

    // コメント種別 (SMALLINT, NN)
    public CommentType CommentType { get; set; }

    // コメント本文 (TEXT, NN)
    public string Body { get; set; } = null!;

    // 申請とのリレーション
    public Request Request { get; set; } = null!;
}