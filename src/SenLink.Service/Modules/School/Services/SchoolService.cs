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
    public async Task<DepartmentListResponse> GetDepartmentsAsync()
    {
        var departments = await departmentRepository.GetAllAsync();
        return new DepartmentListResponse(
            departments.Select(d => new DepartmentDto(d.Id, d.Name, d.Code)).ToList()
        );
    }

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

    public async Task<StudentProfileCreatedResponse> CreateStudentProfileAsync(long accountId, CreateStudentProfileOnboardingRequest request)
    {
        var existingByAccount = await studentRepository.GetByAccountIdAsync(accountId);
        if (existingByAccount != null) return null!;

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
            IsJobHunting = request.IsJobHunting,
            ProfileData = MapToEntity(request.ProfileData, request.Pr)
        };

        await studentRepository.AddAsync(student);

        return new StudentProfileCreatedResponse(
            student.Id,
            student.AccountId,
            student.ClassId,
            student.StudentNumber,
            student.IsJobHunting);
    }

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
            MapToDto(student.ProfileData));
    }

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

    public async Task<bool> UpdateStudentProfileAsync(long accountId, UpdateStudentProfileRequest request)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) return false;

        student.ProfileData = MapToEntity(request.ProfileData, request.Pr, request.Certifications, request.Links);

        await studentRepository.UpdateAsync(student);
        return true;
    }

    public async Task<bool> UpdateJobHuntingStatusAsync(long accountId, UpdateJobHuntingStatusRequest request)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) return false;

        student.IsJobHunting = request.IsJobHunting;

        await studentRepository.UpdateAsync(student);
        return true;
    }

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

    private static StudentProfile MapToEntity(StudentProfileDataDto? dto, string? pr = null, string? certs = null, string? links = null)
    {
        var entity = new StudentProfile
        {
            Pr = pr,
            Certifications = certs,
            Links = links
        };

        if (dto == null) return entity;

        entity.AcademicHistories = dto.AcademicHistories?.Select(a => new AcademicHistory
        {
            SchoolName = a.SchoolName,
            Faculty = a.Faculty,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            Status = a.Status
        }).ToList();

        entity.WorkHistories = dto.WorkHistories?.Select(w => new WorkHistory
        {
            Type = w.Type,
            Organization = w.Organization,
            Role = w.Role,
            Content = w.Content,
            StartDate = w.StartDate,
            EndDate = w.EndDate
        }).ToList();

        entity.CertificationDetails = dto.CertificationDetails?.Select(c => new CertificationDetail
        {
            Name = c.Name,
            Date = c.Date
        }).ToList();

        if (dto.Skills != null)
        {
            entity.Skills = new SkillSet
            {
                Languages = dto.Skills.Languages,
                Frameworks = dto.Skills.Frameworks,
                Others = dto.Skills.Others
            };
        }

        if (dto.SocialLinks != null)
        {
            entity.SocialLinks = new SocialLinks
            {
                Github = dto.SocialLinks.Github,
                Portfolio = dto.SocialLinks.Portfolio,
                Blog = dto.SocialLinks.Blog,
                Twitter = dto.SocialLinks.Twitter
            };
        }

        if (dto.SelfPromotion != null)
        {
            entity.SelfPromotion = new SelfPromotionDetail
            {
                Catchphrase = dto.SelfPromotion.Catchphrase,
                Content = dto.SelfPromotion.Content,
                Strengths = dto.SelfPromotion.Strengths
            };
        }

        return entity;
    }

    private static StudentProfileDataDto? MapToDto(StudentProfile? entity)
    {
        if (entity == null) return null;

        return new StudentProfileDataDto(
            AcademicHistories: entity.AcademicHistories?.Select(a => new AcademicHistoryDto(a.SchoolName, a.Faculty, a.StartDate, a.EndDate, a.Status)).ToList(),
            WorkHistories: entity.WorkHistories?.Select(w => new WorkHistoryDto(w.Type, w.Organization, w.Role, w.Content, w.StartDate, w.EndDate)).ToList(),
            CertificationDetails: entity.CertificationDetails?.Select(c => new CertificationDetailDto(c.Name, c.Date)).ToList(),
            Skills: entity.Skills != null ? new SkillSetDto(entity.Skills.Languages, entity.Skills.Frameworks, entity.Skills.Others) : null,
            SocialLinks: entity.SocialLinks != null ? new SocialLinksDto(entity.SocialLinks.Github, entity.SocialLinks.Portfolio, entity.SocialLinks.Blog, entity.SocialLinks.Twitter) : null,
            SelfPromotion: entity.SelfPromotion != null ? new SelfPromotionDetailDto(entity.SelfPromotion.Catchphrase, entity.SelfPromotion.Content, entity.SelfPromotion.Strengths) : null
        );
    }
}
