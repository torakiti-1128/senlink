using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Auth.Repositories;

/// <summary>
/// ログイン履歴リポジトリの実装
/// </summary>
public class LoginHistoryRepository(SenLinkDbContext context) : ILoginHistoryRepository
{
    public async Task AddAsync(LoginHistory history)
    {
        await context.Set<LoginHistory>().AddAsync(history);
        await context.SaveChangesAsync();
    }
}
