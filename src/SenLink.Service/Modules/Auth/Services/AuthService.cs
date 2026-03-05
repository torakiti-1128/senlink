using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;

namespace SenLink.Service.Modules.Auth.Services;

/// <summary>
/// 認証サービス
/// </summary>
/// <param name="accountRepository">アカウントリポジトリ</param>
/// <param name="tokenService">トークンサービス</param>
public class AuthService(
    IAccountRepository accountRepository, 
    ITokenService tokenService) : IAuthService
{
    /// <summary>
    /// ログイン処理
    /// </summary>
    /// <param name="request">ログインリクエスト</param>
    /// <returns>認証レスポンス</returns>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // DBからユーザーを取得
        var account = await accountRepository.GetByEmailAsync(request.Email);

        // 存在確認 & パスワード検証
        if (account == null || !account.VerifyPassword(request.Password))
            return null;

        // アカウントの状態（有効か、削除されていないか）を確認
        if (!account.IsActive || account.DeletedAt != null)
            return null;

        // トークン生成
        var token = tokenService.CreateToken(account);
        
        // 認証レスポンスを返す
        return new AuthResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(1440), 
            UserId: account.Id,
            Email: account.Email,
            Role: account.Role.ToString()
        );
    }
}