namespace SenLink.Service.Modules.Auth.DTOs;
public record AuthResponse(
    string Token, 
    DateTime ExpiresAt, 
    Guid UserId, 
    string Email, 
    string Role);