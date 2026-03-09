using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.School.Enums;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;

namespace SenLink.Service.Modules.School.Services;

/// <summary>
/// 学校情報およびプロフィール管理サービスのビジネスロジックを実装します
/// </summary>
public class SchoolService(
    IDepartmentRepository departmentRepository,
    IClassRepository classRepository,
    IStudentRepository studentRepository,
    ITeacherRepository teacherRepository) : ISchoolService
{
    /// <summary>
    /// すべての学科を取得します
    /// </summary>
    public async Task<DepartmentListResponse> GetDepartmentsAsync()
    {
        var departments = await departmentRepository.GetAllAsync();
        return new DepartmentListResponse(
            departments.Select(d => new DepartmentDto(d.Id, d.Name, d.Code)).ToList()
        );
    }

    /// <summary>
    /// 条件に合致するクラスを取得します
    /// </summary>
    public async Task<ClassListResponse> GetClassesAsync(long? departmentId, int? fiscalYear, int? grade)
    {
        var classes = await classRepository.GetFilteredAsync(departmentId, fiscalYear, grade);
        
        return new ClassListResponse(
            classes.Select(c => new ClassDto(
                c.Id, 
                c.DepartmentId, 
                c.Department?.Name ?? "Unknown",
                c.FiscalYear,
                c.Grade,
                c.Name)).ToList()
        );
    }

    /// <summary>
    /// 学生プロフィールを作成します。重複チェックを含みます。
    /// </summary>
    public async Task<StudentProfileCreatedResponse> CreateStudentProfileAsync(long accountId, CreateStudentProfileOnboardingRequest request)
    {
        // アカウントごとの重複チェック
        var existingByAccount = await studentRepository.GetByAccountIdAsync(accountId);
        if (existingByAccount != null) return null!;

        // 学籍番号の重複チェック
        var existingByNumber = await studentRepository.GetByStudentNumberAsync(request.StudentNumber);
        if (existingByNumber != null) return null!;

        var student = new Student
        {
            AccountId = accountId,
            ClassId = request.ClassId,
            StudentNumber = request.StudentNumber,
            Name = request.Name,
            NameKana = request.NameKana,
            DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth),
            Gender = (Gender)request.Gender,
            AdmissionYear = request.AdmissionYear,
            IsJobHunting = true
        };

        await studentRepository.AddAsync(student);

        return new StudentProfileCreatedResponse(
            student.Id,
            student.AccountId,
            student.ClassId,
            student.StudentNumber,
            student.IsJobHunting);
    }

    /// <summary>
    /// 教員プロフィールを作成します。重複チェックを含みます。
    /// </summary>
    public async Task<TeacherProfileCreatedResponse> CreateTeacherProfileAsync(long accountId, CreateTeacherProfileOnboardingRequest request)
    {
        var existing = await teacherRepository.GetByAccountIdAsync(accountId);
        if (existing != null) return null!;

        var teacher = new Teacher
        {
            AccountId = accountId,
            Name = request.Name,
            NameKana = request.NameKana,
            Title = request.Title,
            OfficeLocation = request.OfficeLocation
        };

        await teacherRepository.AddAsync(teacher);

        return new TeacherProfileCreatedResponse(teacher.Id, teacher.AccountId);
    }

    /// <summary>
    /// 現在のアカウントIDに紐づく学生プロフィールを取得します
    /// </summary>
    public async Task<StudentMeResponse?> GetStudentMeAsync(long accountId)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) return null;

        var classDto = new ClassDto(
            student.Class.Id,
            student.Class.DepartmentId,
            student.Class.Department.Name,
            student.Class.FiscalYear,
            student.Class.Grade,
            student.Class.Name);

        return new StudentMeResponse(
            student.Id,
            student.AccountId,
            student.StudentNumber,
            student.Name,
            student.NameKana,
            classDto,
            student.DateOfBirth.ToDateTime(TimeOnly.MinValue),
            (short)student.Gender,
            student.AdmissionYear,
            student.IsJobHunting,
            student.ProfileData);
    }

    /// <summary>
    /// 現在のアカウントIDに紐づく教員プロフィールを取得します
    /// </summary>
    public async Task<TeacherMeResponse?> GetTeacherMeAsync(long accountId)
    {
        var teacher = await teacherRepository.GetByAccountIdAsync(accountId);
        if (teacher == null) return null;

        return new TeacherMeResponse(
            teacher.Id,
            teacher.AccountId,
            teacher.Name,
            teacher.NameKana,
            teacher.Title,
            teacher.OfficeLocation,
            teacher.ProfileData);
    }

    /// <summary>
    /// 学生の就活プロフィールを更新します
    /// </summary>
    public async Task<bool> UpdateStudentProfileAsync(long accountId, UpdateStudentProfileRequest request)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) return false;

        student.ProfileData ??= new StudentProfile();
        student.ProfileData.Pr = request.Pr;
        student.ProfileData.Certifications = request.Certifications;
        student.ProfileData.Links = request.Links;

        await studentRepository.UpdateAsync(student);
        return true;
    }

    /// <summary>
    /// 学生の就活中フラグを更新します
    /// </summary>
    public async Task<bool> UpdateJobHuntingStatusAsync(long accountId, UpdateJobHuntingStatusRequest request)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) return false;

        student.IsJobHunting = request.IsJobHunting;

        await studentRepository.UpdateAsync(student);
        return true;
    }

    /// <summary>
    /// 教員のプロフィール情報を更新します
    /// </summary>
    public async Task<bool> UpdateTeacherProfileAsync(long accountId, UpdateTeacherProfileRequest request)
    {
        var teacher = await teacherRepository.GetByAccountIdAsync(accountId);
        if (teacher == null) return false;

        teacher.Title = request.Title;
        teacher.OfficeLocation = request.OfficeLocation;

        teacher.ProfileData ??= new TeacherProfile();
        teacher.ProfileData.Career = request.Career;
        teacher.ProfileData.Speciality = request.Speciality;

        await teacherRepository.UpdateAsync(teacher);
        return true;
    }
}
