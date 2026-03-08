using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Auth.Repositories;

/// <summary>
/// アカウントリポジトリの実装
/// </summary>
public class AccountRepository(SenLinkDbContext context) : IAccountRepository
{
    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await context.Set<Account>()
            .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null);
    }

    public async Task<Account?> GetByIdAsync(long id)
    {
        return await context.Set<Account>().FindAsync(id);
    }

    public async Task AddAsync(Account account)
    {
        await context.Set<Account>().AddAsync(account);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        context.Set<Account>().Update(account);
        await context.SaveChangesAsync();
    }
}
