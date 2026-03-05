using SenLink.Service.Modules.Auth.DTOs;

namespace SenLink.Service.Modules.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}