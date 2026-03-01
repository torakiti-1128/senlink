using SenLink.Domain.Common;

namespace SenLink.Domain.Modules.Audit.Entities;

/// <summary>
/// 監査ログ：操作ログの記録、証跡管理
/// </summary>
public class AuditLog : BaseEntity
{
    // 操作者ID (NOFK, NN, accounts.id)
    public long ActorId { get; set; }

    // 対象テーブル名 (VARCHAR(50), NN)
    public string TargetTable { get; set; } = null!;

    // 対象ID (NN)
    public long TargetId { get; set; }

    // 操作種別 (VARCHAR(50), NN)
    public string Method { get; set; } = null!;

    // 変更前後のデータ (JSONB)
    public AuditLogDetails? Details { get; set; }

    // IPアドレス (VARCHAR(45))
    public string? IpAddress { get; set; }
}

// 監査ログの変更前後のデータ構造
public class AuditLogDetails
{
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
}