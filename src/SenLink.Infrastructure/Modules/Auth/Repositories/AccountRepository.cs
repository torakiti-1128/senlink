using Microsoft.EntityFrameworkCore;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Infrastructure.Persistence;

namespace SenLink.Infrastructure.Modules.Auth.Repositories;

/// <summary>
/// アカウントリポジトリ
/// </summary>
/// <param name="context"></param>
public class AccountRepository(SenLinkDbContext context) : IAccountRepository
{
    /// <summary>
    /// メールアドレスでアカウントを探す
    /// </summary>
    /// <param name="email">メールアドレス</param>
    /// <returns>一致するアカウント情報</returns>
    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await context.Set<Account>()
            .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null);
    }

    /// <summary>
    /// IDでアカウントを探す
    /// </summary>
    /// <param name="id">アカウントID</param>
    /// <returns>一致するアカウント情報</returns>
    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await context.Set<Account>().FindAsync(id);
    }

    /// <summary>
    /// 新規登録
    /// </summary>
    /// <param name="account">アカウント情報</param>
    public async Task AddAsync(Account account)
    {
        await context.Set<Account>().AddAsync(account);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="account">アカウント情報</param>
    public void Update(Account account)
    {
        context.Set<Account>().Update(account);
    }
}