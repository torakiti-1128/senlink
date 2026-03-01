using SenLink.Domain.Common;
using SenLink.Domain.Modules.Notification.Enums;

namespace SenLink.Domain.Modules.Notification.Entities;

/// <summary>
/// LINE連携：ユーザーアカウントとLINEユーザーIDの紐付け情報
/// </summary>
public class AccountLineLink : BaseEntity
{
    // アカウントID (NOFK, NN, UQ, accounts.id)
    public long AccountId { get; set; }

    // LINEのユーザーId (VARCHAR(64), NN)
    public string LineUserId { get; set; } = null!;

    // 連携ステータス (SMALLINT, NN, Default: 0)
    public LineLinkStatus Status { get; set; } = LineLinkStatus.Unlinked;

    // 連携日時 (TIMESTAMP)
    public DateTime? LinkedAt { get; set; }

    // 解除日時 (TIMESTAMP)
    public DateTime? UnlinkedAt { get; set; }
}