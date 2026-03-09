using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.Auth.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // ログイン処理
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // クライアント情報を取得してリクエストを補完
        var fullRequest = request with 
        { 
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var response = await authService.LoginAsync(fullRequest);

        if (response == null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Login successful.",
            Operation = "AUTH_LOGIN",
            Data = response
        });
    }

    // ユーザー登録処理
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (!result)
        {
            throw new BadRequestException("Registration failed. Please check your domain or duplicate email.");
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Registration successful.",
            Operation = "AUTH_REGISTER"
        });
    }

    // OTP要求
    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request)
    {
        var code = await authService.GenerateOtpAsync(request.Email);
        
        // 実際はここでメール送信をトリガーするが、今回はコードを返す（デバッグ用）
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OTP sent.",
            Operation = "AUTH_OTP_REQUEST",
            Data = new { Otp = code }
        });
    }

    // OTP検証
    [HttpPost("otp/verify")]
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

    // パスワードリセット要求
    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        var token = await authService.RequestPasswordResetAsync(request.Email);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Reset token generated.",
            Operation = "AUTH_PASS_RESET_REQUEST",
            Data = new { Token = token }
        });
    }

    // パスワードリセット実行
    [HttpPost("password-reset/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request);

        if (!result)
        {
            throw new BadRequestException("Invalid token or email.");
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Password reset successful.",
            Operation = "AUTH_PASS_RESET_EXECUTE"
        });
    }
}
