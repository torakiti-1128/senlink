using SenLink.Domain.Modules.Audit.Entities;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
}