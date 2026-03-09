using SenLink.Domain.Modules.Auth.Entities;

namespace SenLink.Domain.Modules.Auth.Repositories;

/// <summary>
/// ログイン履歴リポジトリインターフェース
/// </summary>
public interface ILoginHistoryRepository
{
    /// <summary>
    /// ログイン履歴を保存する
    /// </summary>
    /// <param name="history">履歴情報</param>
    Task AddAsync(LoginHistory history);
}
