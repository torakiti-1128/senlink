using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.Auth.DTOs;
using SenLink.Service.Modules.Auth.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Extensions;
using SenLink.Shared.Results;

namespace SenLink.Api.Modules.Auth.Controllers;

[ApiController]
[Route("api/v1/auth/otp")]
public class OtpController(IAuthService authService) : ControllerBase
{
    [HttpPost("request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request)
    {
        var result = await authService.GenerateOtpAsync(request.Email, request.Purpose);
        
        if (!result.IsSuccess) return result.ToActionResult("AUTH_OTP_REQUEST");

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = (int)result.StatusCode,
            Message = "OTP sent.",
            Operation = "AUTH_OTP_REQUEST",
#if DEBUG
            Data = new { Otp = result.Data }
#endif
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await authService.VerifyOtpAndLoginAsync(request, request.Purpose);

        return result.ToActionResult("AUTH_OTP_VERIFY");
    }
}
