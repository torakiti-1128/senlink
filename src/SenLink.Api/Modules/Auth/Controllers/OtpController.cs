using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.Auth.Controllers;

/// <summary>
/// ワンタイムパスワード（OTP）に関連する認証を管理するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/auth/otp")]
public class OtpController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// OTP要求
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request)
    {
        var code = await authService.GenerateOtpAsync(request.Email);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OTP sent.",
            Operation = "AUTH_OTP_REQUEST",
            Data = new { Otp = code }
        });
    }

    /// <summary>
    /// OTP検証
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await authService.VerifyOtpAsync(request);

        if (!result)
        {
            throw new BadRequestException("Invalid or expired OTP.");
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OTP verified.",
            Operation = "AUTH_OTP_VERIFY"
        });
    }
}
