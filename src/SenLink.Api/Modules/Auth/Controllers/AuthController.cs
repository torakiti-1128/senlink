using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.Auth.Controllers;

/// <summary>
/// 基本的な認証（ログイン・登録）を管理するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// ログイン処理
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
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

    /// <summary>
    /// ユーザー登録処理
    /// </summary>
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
}
