using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;
using System.Security.Claims;

namespace SenLink.Api.Modules.School.Controllers;

/// <summary>
/// 教員のプロフィールおよびオンボーディングを管理するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/school/teachers")]
[Authorize]
public class TeacherController(ISchoolService schoolService) : ControllerBase
{
    /// <summary>
    /// 初回ログイン時のプロフィール登録を行います
    /// </summary>
    /// <param name="request">登録内容</param>
    /// <returns>作成されたプロフィール情報</returns>
    [HttpPost("onboarding")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateTeacherProfile([FromBody] CreateTeacherProfileOnboardingRequest request)
    {
        long accountId = GetCurrentAccountId();
        var response = await schoolService.CreateTeacherProfileAsync(accountId, request);

        if (response == null)
        {
            throw new ConflictException("Teacher profile already exists.");
        }

        return Created("", new ApiResponse<TeacherProfileCreatedResponse>
        {
            Success = true,
            Code = StatusCodes.Status201Created,
            Message = "Teacher profile created",
            Operation = "SCHOOL_TEACHER_ONBOARDING_CREATE",
            Data = response
        });
    }

    /// <summary>
    /// 自身のプロフィール情報を取得します
    /// </summary>
    /// <returns>教員プロフィール詳細</returns>
    [HttpGet("me")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetTeacherMe()
    {
        long accountId = GetCurrentAccountId();
        var response = await schoolService.GetTeacherMeAsync(accountId);

        if (response == null)
        {
            throw new NotFoundException("Teacher profile not found.");
        }

        return Ok(new ApiResponse<TeacherMeResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OK",
            Operation = "SCHOOL_TEACHER_ME_GET",
            Data = response
        });
    }

    /// <summary>
    /// 教員プロフィール情報を更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("me/profile")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateTeacherProfile([FromBody] UpdateTeacherProfileRequest request)
    {
        long accountId = GetCurrentAccountId();
        var success = await schoolService.UpdateTeacherProfileAsync(accountId, request);

        if (!success) throw new NotFoundException("Teacher profile not found.");

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Profile updated",
            Operation = "SCHOOL_TEACHER_ME_PROFILE_UPDATE"
        });
    }

    private long GetCurrentAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
}
