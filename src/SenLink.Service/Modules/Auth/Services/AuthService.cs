using System.ComponentModel;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Enums;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Maintenance.Interfeces;
using SenLink.Service.Common.Interfaces;

namespace SenLink.Service.Modules.Auth.Services;

public class AuthService(
    IAccountRepository accountRepository, 
    ILoginHistoryRepository loginHistoryRepository,
    IOneTimePasswordRepository otpRepository,
    ITokenService tokenService,
    ISystemSettingProvider settingProvider,
    IEmailSender emailSender) : IAuthService
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var account = await accountRepository.GetByEmailAsync(request.Email);
        if (account == null) return null;

        bool isPasswordValid = account.VerifyPassword(request.Password);
        bool isUserActive = account.IsActive && account.DeletedAt == null;
        bool isSuccess = isPasswordValid && isUserActive;

        var history = new LoginHistory
        {
            AccountId = account.Id,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Status = isSuccess ? LoginStatus.Success : LoginStatus.Failure
        };
        await loginHistoryRepository.AddAsync(history);

        if (!isSuccess) return null;

        var token = tokenService.CreateToken(account);
        
        return new AuthResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(1440), 
            UserId: account.Id,
            Email: account.Email,
            Role: account.Role.ToString()
        );
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existing = await accountRepository.GetByEmailAsync(request.Email);
        if (existing != null) return false;

        var allowedDomainsStr = settingProvider.GetValue("AllowedEmailDomains") ?? "senlink.dev";
        var allowedDomains = allowedDomainsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        try
        {
            var account = Account.Create(request.Email, request.Password, allowedDomains);
            await accountRepository.AddAsync(account);
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    public async Task<string> GenerateOtpAsync(string email, string purpose)
    {
        int optLength = int.TryParse(settingProvider.GetValue("OtpCodeLength"), out int length) ? length : 6;
        var otp = new OneTimePassword().CreateOTP(email, purpose, optLength);
        await otpRepository.AddAsync(otp);

        string subject = purpose switch {
            "Register" => "【SenLink】新規登録認証コード",
            "PasswordReset" => "【SenLink】パスワード再設定コード",
            _ => "【SenLink】認証コード"
        };

        await emailSender.SendEmailAsync(email, subject, $"あなたの認証コードは {otp.Code} です。");
        return otp.Code;
    }

    public async Task<AuthResponse?> VerifyOtpAndLoginAsync(VerifyOtpRequest request, string purpose)
    {
        var otp = await otpRepository.GetValidOtpAsync(request.Email, request.Otp, purpose);
        if (otp == null) return null;

        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp);

        var account = await accountRepository.GetByEmailAsync(request.Email);
        if (account == null) return null;

        var token = tokenService.CreateToken(account);
        return new AuthResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(1440),
            UserId: account.Id,
            Email: account.Email,
            Role: account.Role.ToString()
        );
    }

    public async Task<string> RequestPasswordResetAsync(string email)
    {
        var account = await accountRepository.GetByEmailAsync(email);
        if (account == null) return string.Empty;

        return await GenerateOtpAsync(email, "PasswordReset");
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // 修正: 検証済み(IsUsed=true)であっても有効期限内なら許可する
        var otp = await otpRepository.GetAnyByTokenAsync(request.Token, "PasswordReset");
        
        // OTPが見つからない、またはメールアドレスが一致しない場合はエラー
        if (otp == null || otp.Email != request.Email) return false;

        var account = await accountRepository.GetByEmailAsync(otp.Email);
        if (account == null) return false;

        account.SetPassword(request.NewPassword);
        await accountRepository.UpdateAsync(account);

        // 念のため再度使用済みとして更新（既になっていればそのまま）
        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp);
        return true;
    }
}
