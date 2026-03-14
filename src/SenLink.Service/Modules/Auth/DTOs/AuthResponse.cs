namespace SenLink.Service.Modules.Auth.DTOs;
public record AuthResponse(
    string Token, 
    DateTime ExpiresAt, 
    long UserId, 
    string Email, 
    string Role,
    bool HasProfile = false);