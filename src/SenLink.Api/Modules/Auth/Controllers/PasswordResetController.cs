using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Extensions;
using SenLink.Shared.Results;

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
        var result = await authService.RequestPasswordResetAsync(request.Email);

        if (!result.IsSuccess) return result.ToActionResult("AUTH_PASSWORD_RESET_REQUEST");

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = (int)result.StatusCode,
            Message = "OTP sent.",
            Operation = "AUTH_PASSWORD_RESET_REQUEST",
#if DEBUG
            Data = new { Token = result.Data }
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
        return result.ToActionResult("AUTH_PASSWORD_RESET_EXECUTE");
    }
}
