using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Audit.Repositories;

public class AuditLogRepository(SenLinkDbContext context) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog auditLog)
    {
        await context.AuditLogs.AddAsync(auditLog);
        await context.SaveChangesAsync();
    }
}

public class ErrorLogRepository(SenLinkDbContext context) : IErrorLogRepository
{
    public async Task AddAsync(ErrorLog errorLog)
    {
        await context.ErrorLogs.AddAsync(errorLog);
        await context.SaveChangesAsync();
    }
}
