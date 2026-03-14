using System.Net;
using SenLink.Domain.Modules.Auth.Entities;
using SenLink.Domain.Modules.Auth.Enums;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Service.Modules.Maintenance.Interfeces;
using SenLink.Service.Common.Interfaces;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Shared.Results;

namespace SenLink.Service.Modules.Auth.Services;

public class AuthService(
    IAccountRepository accountRepository, 
    ILoginHistoryRepository loginHistoryRepository,
    IOneTimePasswordRepository otpRepository,
    ITokenService tokenService,
    ISystemSettingProvider settingProvider,
    IEmailSender emailSender,
    IStudentRepository studentRepository,
    ITeacherRepository teacherRepository) : IAuthService
{
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var account = await accountRepository.GetByEmailAsync(request.Email);
        if (account == null) 
            return Result<AuthResponse>.Failure("Invalid email or password.", HttpStatusCode.Unauthorized, "UNAUTHORIZED_ERROR");

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

        if (!isSuccess) 
            return Result<AuthResponse>.Failure("Invalid email or password.", HttpStatusCode.Unauthorized, "UNAUTHORIZED_ERROR");

        // プロフィールの有無を確認
        bool hasProfile = false;
        if (account.Role == AccountRole.Student)
        {
            var student = await studentRepository.GetByAccountIdAsync(account.Id);
            hasProfile = student != null;
        }
        else if (account.Role == AccountRole.Teacher)
        {
            var teacher = await teacherRepository.GetByAccountIdAsync(account.Id);
            hasProfile = teacher != null;
        }

        var token = tokenService.CreateToken(account);
        
        var response = new AuthResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(1440), 
            UserId: account.Id,
            Email: account.Email,
            Role: account.Role.ToString(),
            HasProfile: hasProfile
        );

        return Result<AuthResponse>.Success(response, "Login successful.");
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var existing = await accountRepository.GetByEmailAsync(request.Email);
        if (existing != null) 
            return Result.Failure("Email already exists.", HttpStatusCode.Conflict, "CONFLICT_ERROR");

        var allowedDomainsStr = settingProvider.GetValue("AllowedEmailDomains") ?? "senlink.dev";
        var allowedDomains = allowedDomainsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        try
        {
            var account = Account.Create(request.Email, request.Password, allowedDomains);
            await accountRepository.AddAsync(account);
            return Result.Success("Registration successful.");
        }
        catch (ArgumentException ex) 
        { 
            return Result.Failure(ex.Message, HttpStatusCode.BadRequest, "BAD_REQUEST_ERROR"); 
        }
    }

    public async Task<Result<string>> GenerateOtpAsync(string email, string purpose)
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
        return Result<string>.Success(otp.Code, "OTP generated and sent.");
    }

    public async Task<Result<AuthResponse>> VerifyOtpAndLoginAsync(VerifyOtpRequest request, string purpose)
    {
        var otp = await otpRepository.GetValidOtpAsync(request.Email, request.Otp, purpose);
        if (otp == null) 
            return Result<AuthResponse>.Failure("Invalid or expired OTP.", HttpStatusCode.BadRequest, "INVALID_OTP_ERROR");

        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp);

        var account = await accountRepository.GetByEmailAsync(request.Email);
        if (account == null) 
            return Result<AuthResponse>.Failure("Account not found.", HttpStatusCode.NotFound, "NOT_FOUND_ERROR");

        // 検証成功時もプロフィールの有無を返す
        bool hasProfile = false;
        if (account.Role == AccountRole.Student)
        {
            var student = await studentRepository.GetByAccountIdAsync(account.Id);
            hasProfile = student != null;
        }
        else if (account.Role == AccountRole.Teacher)
        {
            var teacher = await teacherRepository.GetByAccountIdAsync(account.Id);
            hasProfile = teacher != null;
        }

        var token = tokenService.CreateToken(account);
        var response = new AuthResponse(
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(1440),
            UserId: account.Id,
            Email: account.Email,
            Role: account.Role.ToString(),
            HasProfile: hasProfile
        );

        return Result<AuthResponse>.Success(response, "OTP verified and login successful.");
    }

    public async Task<Result<string>> RequestPasswordResetAsync(string email)
    {
        var account = await accountRepository.GetByEmailAsync(email);
        if (account == null) 
            return Result<string>.Failure("Account with this email does not exist.", HttpStatusCode.NotFound, "NOT_FOUND_ERROR");

        return await GenerateOtpAsync(email, "PasswordReset");
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var otp = await otpRepository.GetAnyByTokenAsync(request.Token, "PasswordReset");
        if (otp == null || otp.Email != request.Email) 
            return Result.Failure("Invalid token or email.", HttpStatusCode.BadRequest, "BAD_REQUEST_ERROR");

        var account = await accountRepository.GetByEmailAsync(otp.Email);
        if (account == null) 
            return Result.Failure("Account not found.", HttpStatusCode.NotFound, "NOT_FOUND_ERROR");

        account.SetPassword(request.NewPassword);
        await accountRepository.UpdateAsync(account);

        otp.IsUsed = true;
        await otpRepository.UpdateAsync(otp);
        return Result.Success("Password reset successful.");
    }
}
