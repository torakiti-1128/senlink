using Microsoft.AspNetCore.Mvc;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Api.Models;
using SenLink.Api.Middlewares;

namespace SenLink.Api.Modules.School.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SchoolController(ISchoolService schoolService) : ControllerBase
{
    // 学科一覧取得
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

    // クラス一覧取得
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

    // 初回プロフィール登録（学生）
    [HttpPost("onboarding/student-profile")]
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

    // 初回プロフィール登録（教員）
    [HttpPost("onboarding/teacher-profile")]
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

    // 自分の学生プロフィール取得
    [HttpGet("students/me")]
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

    private long GetCurrentAccountId()
    {
        // TODO: テストのために一時的にヘッダーから取得できるようにする、またはクレームから取得
        var headerId = Request.Headers["X-Account-Id"].ToString();
        if (!string.IsNullOrEmpty(headerId)) return long.Parse(headerId);

        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
}
