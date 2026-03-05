namespace SenLink.Domain.Modules.Audit.Contracts;

/// <summary>
/// 監査ログが生成されたことを通知するイベントメッセージ
/// </summary>
public record AuditLogCreatedEvent(
    long ActorId,
    string TargetTable,
    long TargetId,
    string Method, // "CREATE", "UPDATE", "DELETE" など
    Dictionary<string, object>? OldValues,
    Dictionary<string, object>? NewValues,
    string? IpAddress,
    DateTime CreatedAt
);