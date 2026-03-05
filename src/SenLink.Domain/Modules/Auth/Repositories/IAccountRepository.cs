using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Domain.Modules.Auth.Repositories;

/// <summary>
/// アカウントリポジトリインターフェース
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// メールアドレスでアカウントを探す
    /// </summary>
    /// <param name="email">メールアドレス</param>
    /// <returns>一致するアカウント情報</returns>
    Task<Account?> GetByEmailAsync(string email);

    /// <summary>
    /// IDでアカウントを探す
    /// </summary>
    /// <param name="id">アカウントID</param>
    /// <returns>一致するアカウント情報</returns>
    Task<Account?> GetByIdAsync(long id);

    /// <summary>
    /// 新規登録
    /// </summary>
    /// <param name="account">アカウント情報</param>
    Task AddAsync(Account account);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="account">アカウント情報</param>
    void Update(Account account);
}