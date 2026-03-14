using SenLink.Service.Modules.Auth.DTOs;

namespace SenLink.Service.Modules.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string> GenerateOtpAsync(string email, string purpose);
    Task<AuthResponse?> VerifyOtpAndLoginAsync(VerifyOtpRequest request, string purpose);
    Task<string> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}
