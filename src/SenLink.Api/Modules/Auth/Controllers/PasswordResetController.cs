using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.Auth.Controllers;

/// <summary>
/// パスワードリセットに関連する認証を管理するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/auth/password-reset")]
public class PasswordResetController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// パスワードリセット要求
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        var token = await authService.RequestPasswordResetAsync(request.Email);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Reset token generated.",
            Operation = "AUTH_PASSWORD_RESET_REQUEST",
#if DEBUG
            Data = new { Token = token }
#endif
        });
    }

    /// <summary>
    /// パスワードリセット実行
    /// </summary>
    [HttpPost("reset")]
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
            Operation = "AUTH_PASSWORD_RESET_EXECUTE"
        });
    }
}
