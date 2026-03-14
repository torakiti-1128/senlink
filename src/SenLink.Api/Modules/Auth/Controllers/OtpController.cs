using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.Auth.Controllers;

[ApiController]
[Route("api/v1/auth/otp")]
public class OtpController(IAuthService authService) : ControllerBase
{
    [HttpPost("request")]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequestDto request)
    {
        var code = await authService.GenerateOtpAsync(request.Email, request.Purpose);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OTP sent.",
            Operation = "AUTH_OTP_REQUEST",
#if DEBUG
            Data = new { Otp = code }
#endif
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequestDto request)
    {
        var authResponse = await authService.VerifyOtpAndLoginAsync(
            new VerifyOtpRequest(request.Email, request.Otp), 
            request.Purpose);

        if (authResponse == null)
        {
            throw new BadRequestException("Invalid or expired OTP.");
        }

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OTP verified.",
            Operation = "AUTH_OTP_VERIFY",
            Data = authResponse
        });
    }
}

public record OtpRequestDto(string Email, string Purpose);
public record OtpVerifyRequestDto(string Email, string Otp, string Purpose);
