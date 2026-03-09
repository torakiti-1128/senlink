namespace SenLink.Service.Modules.Auth.DTOs;

public record LoginRequest(string Email, string Password, string? IpAddress = null, string? UserAgent = null);
