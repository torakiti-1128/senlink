using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;
using System.Security.Claims;

namespace SenLink.Api.Modules.School.Controllers;

/// <summary>
/// 学校情報（学科・クラス）および学生・教員のプロフィールを管理するコントローラー
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SchoolController(ISchoolService schoolService) : ControllerBase
{
    /// <summary>
    /// 学科の一覧を取得します
    /// </summary>
    /// <returns>学科リスト</returns>
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var response = await schoolService.GetDepartmentsAsync();
        
        return Ok(new ApiResponse<DepartmentListResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OK",
            Operation = "school_departments_list",
            Data = response
        });
    }

    /// <summary>
    /// クラスの一覧を取得します。学科、年度、学年での絞り込みが可能です。
    /// </summary>
    /// <param name="departmentId">学科ID</param>
    /// <param name="fiscalYear">年度</param>
    /// <param name="grade">学年</param>
    /// <returns>クラスリスト</returns>
    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses(
        [FromQuery] long? departmentId, 
        [FromQuery] int? fiscalYear, 
        [FromQuery] int? grade)
    {
        var response = await schoolService.GetClassesAsync(departmentId, fiscalYear, grade);

        return Ok(new ApiResponse<ClassListResponse>
        {
            Success = true,
            Code = StatusCodes.Status200OK,
            Message = "OK",
            Operation = "school_classes_list",
            Data = response
        });
    }

    /// <summary>
    /// 【学生専用】初回ログイン時のプロフィール登録を行います
    /// </summary>
    /// <param name="request">登録内容</param>
    /// <returns>作成されたプロフィール情報</returns>
    [HttpPost("onboarding/student-profile")]
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
            Operation = "school_onboarding_student_profile_create",
            Data = response
        });
    }

    /// <summary>
    /// 【教員専用】初回ログイン時のプロフィール登録を行います
    /// </summary>
    /// <param name="request">登録内容</param>
    /// <returns>作成されたプロフィール情報</returns>
    [HttpPost("onboarding/teacher-profile")]
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
            Operation = "school_onboarding_teacher_profile_create",
            Data = response
        });
    }

    /// <summary>
    /// 【学生専用】自身のプロフィール情報を取得します
    /// </summary>
    /// <returns>学生プロフィール詳細</returns>
    [HttpGet("students/me")]
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
            Operation = "school_students_me_get",
            Data = response
        });
    }

    /// <summary>
    /// 【教員専用】自身のプロフィール情報を取得します
    /// </summary>
    /// <returns>教員プロフィール詳細</returns>
    [HttpGet("teachers/me")]
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
            Operation = "school_teachers_me_get",
            Data = response
        });
    }

    /// <summary>
    /// 【学生専用】就活プロフィール情報を更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("students/me/profile")]
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
            Operation = "school_students_me_profile_update"
        });
    }

    /// <summary>
    /// 【学生専用】就活中フラグを更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("students/me/job-hunting")]
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
            Operation = "school_students_me_job_hunting_update"
        });
    }

    /// <summary>
    /// 【教員専用】教員プロフィール情報を更新します
    /// </summary>
    /// <param name="request">更新内容</param>
    /// <returns>成功レスポンス</returns>
    [HttpPatch("teachers/me/profile")]
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
            Operation = "school_teachers_me_profile_update"
        });
    }

    /// <summary>
    /// 認証トークンのクレームから現在のアカウントIDを取得します
    /// </summary>
    private long GetCurrentAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
}
