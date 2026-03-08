namespace SenLink.Domain.Modules.Audit.Contracts;

public record AuditLogCreatedEvent(
    long ActorId,
    string TargetTable,
    long TargetId,
    string Method,
    Dictionary<string, object> OldValues,
    Dictionary<string, object> NewValues,
    string? IpAddress,
    DateTime CreatedAt
);
