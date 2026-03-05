using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Infrastructure.Persistence;

/// <summary>
/// 監査ログリポジトリ
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly SenLinkDbContext _context;
    public AuditLogRepository(SenLinkDbContext context) => _context = context;

    /// <summary>
    /// 監査ログを追加
    /// </summary>
    /// <param name="auditLog">追加する監査ログ</param>
    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}