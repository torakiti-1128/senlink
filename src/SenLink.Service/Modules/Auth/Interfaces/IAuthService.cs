using SenLink.Service.Modules.Auth.DTOs;

namespace SenLink.Service.Modules.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string> GenerateOtpAsync(string email);
    Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
    Task<string> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}