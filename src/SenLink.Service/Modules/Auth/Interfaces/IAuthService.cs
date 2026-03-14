using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Shared.Results;

namespace SenLink.Service.Modules.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result<string>> GenerateOtpAsync(string email, string purpose);
    Task<Result<AuthResponse>> VerifyOtpAndLoginAsync(VerifyOtpRequest request, string purpose);
    Task<Result<string>> RequestPasswordResetAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
}
