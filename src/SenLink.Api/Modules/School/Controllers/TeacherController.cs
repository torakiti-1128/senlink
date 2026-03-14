using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Extensions;
using SenLink.Shared.Constants;
using System.Security.Claims;
using System.Net;

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
    [Authorize(Policy = AuthPolicies.RequireTeacher)]
    public async Task<IActionResult> CreateTeacherProfile([FromBody] CreateTeacherProfileOnboardingRequest request)
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.CreateTeacherProfileAsync(accountId, request);

        if (!result.IsSuccess) return result.ToActionResult("SCHOOL_TEACHER_ONBOARDING_CREATE");

        return Created("", new ApiResponse<TeacherProfileCreatedResponse>
        {
            Success = true,
            Code = (int)HttpStatusCode.Created,
            Message = result.Message,
            Operation = "SCHOOL_TEACHER_ONBOARDING_CREATE",
            Data = result.Data
        });
    }

    /// <summary>
    /// 自身のプロフィール情報を取得します
    /// </summary>
    /// <returns>教員プロフィール詳細</returns>
    [HttpGet("me")]
    [Authorize(Policy = AuthPolicies.RequireTeacher)]
    public async Task<IActionResult> GetTeacherMe()
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.GetTeacherMeAsync(accountId);
        return result.ToActionResult("SCHOOL_TEACHER_ME_GET");
    }

    /// <summary>
    /// 教員プロフィール情報を更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("me/profile")]
    [Authorize(Policy = AuthPolicies.RequireTeacher)]
    public async Task<IActionResult> UpdateTeacherProfile([FromBody] UpdateTeacherProfileRequest request)
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.UpdateTeacherProfileAsync(accountId, request);
        return result.ToActionResult("SCHOOL_TEACHER_ME_PROFILE_UPDATE");
    }

    private long GetCurrentAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
}
