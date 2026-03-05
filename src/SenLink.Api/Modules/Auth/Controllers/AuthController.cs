using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // ログイン処理
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // サービスの呼び出し
        var response = await authService.LoginAsync(request);

        // 認証失敗時はカスタム例外をスロー（GlobalExceptionHandlerが自動処理）
        if (response == null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        // 成功レスポンスの構築
        var apiResponse = new ApiResponse<AuthResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Login successful.",
            Operation = "LOGIN",
            Data = response
        };

        return Ok(apiResponse);
    }
}