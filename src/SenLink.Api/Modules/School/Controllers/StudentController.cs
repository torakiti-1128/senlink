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
/// 学生のプロフィールおよびオンボーディングを管理するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/school/students")]
[Authorize]
public class StudentController(ISchoolService schoolService) : ControllerBase
{
    /// <summary>
    /// 初回ログイン時のプロフィール登録を行います
    /// </summary>
    /// <param name="request">登録内容</param>
    /// <returns>作成されたプロフィール情報</returns>
    [HttpPost("onboarding")]
    [Authorize(Policy = AuthPolicies.RequireStudent)]
    public async Task<IActionResult> CreateStudentProfile([FromBody] CreateStudentProfileOnboardingRequest request)
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.CreateStudentProfileAsync(accountId, request);

        if (!result.IsSuccess) return result.ToActionResult("SCHOOL_STUDENT_ONBOARDING_CREATE");

        return Created("", new ApiResponse<StudentProfileCreatedResponse>
        {
            Success = true,
            Code = (int)HttpStatusCode.Created,
            Message = result.Message,
            Operation = "SCHOOL_STUDENT_ONBOARDING_CREATE",
            Data = result.Data
        });
    }

    /// <summary>
    /// 自身のプロフィール情報を取得します
    /// </summary>
    /// <returns>学生プロフィール詳細</returns>
    [HttpGet("me")]
    [Authorize(Policy = AuthPolicies.RequireStudent)]
    public async Task<IActionResult> GetStudentMe()
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.GetStudentMeAsync(accountId);
        return result.ToActionResult("SCHOOL_STUDENT_ME_GET");
    }

    /// <summary>
    /// 就活プロフィール情報を更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("me/profile")]
    [Authorize(Policy = AuthPolicies.RequireStudent)]
    public async Task<IActionResult> UpdateStudentProfile([FromBody] UpdateStudentProfileRequest request)
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.UpdateStudentProfileAsync(accountId, request);
        return result.ToActionResult("SCHOOL_STUDENT_ME_PROFILE_UPDATE");
    }

    /// <summary>
    /// 就活中フラグを更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("me/job-hunting")]
    [Authorize(Policy = AuthPolicies.RequireStudent)]
    public async Task<IActionResult> UpdateJobHuntingStatus([FromBody] UpdateJobHuntingStatusRequest request)
    {
        long accountId = GetCurrentAccountId();
        var result = await schoolService.UpdateJobHuntingStatusAsync(accountId, request);
        return result.ToActionResult("SCHOOL_STUDENT_ME_JOB_HUNTING_UPDATE");
    }

    private long GetCurrentAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
}
