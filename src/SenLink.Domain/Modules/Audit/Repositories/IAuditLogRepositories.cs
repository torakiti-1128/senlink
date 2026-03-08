using SenLink.Domain.Modules.Audit.Entities;

namespace SenLink.Domain.Modules.Audit.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
}

public interface IErrorLogRepository
{
    Task AddAsync(ErrorLog errorLog);
}
