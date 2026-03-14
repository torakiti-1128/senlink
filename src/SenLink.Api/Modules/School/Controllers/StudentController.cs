using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;
using System.Security.Claims;

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
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateStudentProfile([FromBody] CreateStudentProfileOnboardingRequest request)
    {
        long accountId = GetCurrentAccountId();
        var response = await schoolService.CreateStudentProfileAsync(accountId, request);

        if (response == null)
        {
            throw new ConflictException("Student profile already exists or student number is duplicated.");
        }

        return Created("", new ApiResponse<StudentProfileCreatedResponse>
        {
            Success = true,
            Code = StatusCodes.Status201Created,
            Message = "Student profile created",
            Operation = "SCHOOL_STUDENT_ONBOARDING_CREATE",
            Data = response
        });
    }

    /// <summary>
    /// 自身のプロフィール情報を取得します
    /// </summary>
    /// <returns>学生プロフィール詳細</returns>
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentMe()
    {
        long accountId = GetCurrentAccountId();
        var response = await schoolService.GetStudentMeAsync(accountId);

        if (response == null)
        {
            throw new NotFoundException("Student profile not found.");
        }

        return Ok(new ApiResponse<StudentMeResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OK",
            Operation = "SCHOOL_STUDENT_ME_GET",
            Data = response
        });
    }

    /// <summary>
    /// 就活プロフィール情報を更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("me/profile")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UpdateStudentProfile([FromBody] UpdateStudentProfileRequest request)
    {
        long accountId = GetCurrentAccountId();
        var success = await schoolService.UpdateStudentProfileAsync(accountId, request);

        if (!success) throw new NotFoundException("Student profile not found.");

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Profile updated",
            Operation = "SCHOOL_STUDENT_ME_PROFILE_UPDATE"
        });
    }

    /// <summary>
    /// 就活中フラグを更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("me/job-hunting")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> UpdateJobHuntingStatus([FromBody] UpdateJobHuntingStatusRequest request)
    {
        long accountId = GetCurrentAccountId();
        var success = await schoolService.UpdateJobHuntingStatusAsync(accountId, request);

        if (!success) throw new NotFoundException("Student profile not found.");

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "Job hunting status updated",
            Operation = "SCHOOL_STUDENT_ME_JOB_HUNTING_UPDATE"
        });
    }

    private long GetCurrentAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
}
