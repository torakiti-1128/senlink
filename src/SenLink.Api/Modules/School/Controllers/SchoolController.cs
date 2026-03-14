using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Api.Models;

namespace SenLink.Api.Modules.School.Controllers;

/// <summary>
/// 学校情報（学科・クラス）を管理するコントローラー
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
            Operation = "SCHOOL_DEPARTMENTS_LIST",
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
            Operation = "SCHOOL_CLASSES_LIST",
            Data = response
        });
    }
}
