using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Maintenance.Interfeces;

namespace SenLink.Service.Modules.Auth.Services;

/// <summary>
/// 認証サービス
/// </summary>
public class AuthService(
    IAccountRepository accountRepository, 
    IOneTimePasswordRepository otpRepository,
    ITokenService tokenService,
    ISystemSettingProvider settingProvider) : IAuthService
{
    /// <summary>
    /// ログイン処理
    /// </summary>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var account = await accountRepository.GetByEmailAsync(request.Email);

        if (account == null || !account.VerifyPassword(request.Password))
            return null;

        if (!account.IsActive || account.DeletedAt != null)
            return null;

        var token = tokenService.CreateToken(account);
        
        return new AuthResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(1440), 
            UserId: account.Id,
            Email: account.Email,
            Role: account.Role.ToString()
        );
    }

    /// <summary>
    /// ユーザー登録処理
    /// </summary>
    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        // 1. ドメイン制限チェック（ドメイン層のルールを呼び出し）
        var allowedDomainsStr = settingProvider.GetValue("AllowedEmailDomains") ?? "senlink.dev";
        var allowedDomains = allowedDomainsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (!Account.IsValidEmailDomain(request.Email, allowedDomains))
        {
            return false;
        }

        // 2. 重複チェック
        var existing = await accountRepository.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            return false;
        }

        // 3. アカウント作成
        var account = new Account
        {
            Email = request.Email,
            Role = AccountRole.Student,
            IsActive = true
        };
        account.SetPassword(request.Password);

        await accountRepository.AddAsync(account);
        return true;
    }

    /// <summary>
    /// OTP生成
    /// </summary>
    public async Task<string> GenerateOtpAsync(string email)
    {
        var code = new Random().Next(100000, 999999).ToString();
        
        var otp = new OneTimePassword
        {
            Email = email,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Purpose = "Register"
        };

        await otpRepository.AddAsync(otp);
        return code;
    }

    /// <summary>
    /// OTP検証
    /// </summary>
    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var otp = await otpRepository.GetValidOtpAsync(request.Email, request.Otp, "Register");
        if (otp == null) return false;

        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp);
        return true;
    }

    /// <summary>
    /// パスワードリセット要求
    /// </summary>
    public async Task<string> RequestPasswordResetAsync(string email)
    {
        var account = await accountRepository.GetByEmailAsync(email);
        if (account == null) return string.Empty;

        var token = Guid.NewGuid().ToString("N");
        
        var otp = new OneTimePassword
        {
            Email = email,
            Code = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Purpose = "PasswordReset"
        };

        await otpRepository.AddAsync(otp);
        return token;
    }
/// <summary>
/// パスワードリセット実行
/// </summary>
public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
{
    // OneTimePasswordテーブルをトークン保存に流用
    var otp = await otpRepository.GetValidByTokenAsync(request.Token, "PasswordReset");
    if (otp == null) return false;


        var account = await accountRepository.GetByEmailAsync(otp.Email);
        if (account == null) return false;

        account.SetPassword(request.NewPassword);
        await accountRepository.UpdateAsync(account);

        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp);

        return true;
    }
}
