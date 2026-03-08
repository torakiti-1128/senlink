using SenLink.Domain.Modules.Audit.Entities;
using SenLink.Domain.Modules.Audit.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Audit.Repositories;

/// <summary>
/// エラーログリポジトリの実装
/// </summary>
public class ErrorLogRepository : IErrorLogRepository
{
    private readonly SenLinkDbContext _context;

    public ErrorLogRepository(SenLinkDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// エラーログを追加する
    /// </summary>
    /// <param name="errorLog"></param>
    /// <returns></returns>
    public async Task AddAsync(ErrorLog errorLog)
    {
        await _context.ErrorLogs.AddAsync(errorLog);
        await _context.SaveChangesAsync();
    }
}
