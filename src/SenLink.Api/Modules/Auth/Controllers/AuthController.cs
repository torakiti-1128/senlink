using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Extensions;

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

        var result = await authService.LoginAsync(fullRequest);
        return result.ToActionResult("AUTH_LOGIN");
    }

    /// <summary>
    /// ユーザー登録処理
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return result.ToActionResult("AUTH_REGISTER");
    }
}
