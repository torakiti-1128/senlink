namespace SenLink.Service.Modules.Auth.DTOs;

public record RegisterRequest(string Email, string Password);

public record RequestOtpRequest(string Email);

public record VerifyOtpRequest(string Email, string Otp);

public record RequestPasswordResetRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);
