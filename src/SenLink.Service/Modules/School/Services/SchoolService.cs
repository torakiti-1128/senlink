using System.Net;
using SenLink.Domain.Modules.School.Entities;
using SenLink.Domain.Modules.School.Enums;
using SenLink.Domain.Modules.School.Repositories;
using SenLink.Domain.Modules.Auth.Repositories;
using SenLink.Service.Modules.School.DTOs;
using SenLink.Service.Modules.School.Interfaces;
using SenLink.Shared.Results;

namespace SenLink.Service.Modules.School.Services;

/// <summary>
/// 学校情報およびプロフィール管理サービスのビジネスロジックを実装します
/// </summary>
public class SchoolService(
    IDepartmentRepository departmentRepository,
    IClassRepository classRepository,
    IStudentRepository studentRepository,
    ITeacherRepository teacherRepository,
    IAccountRepository accountRepository) : ISchoolService
{
    public async Task<Result<DepartmentListResponse>> GetDepartmentsAsync()
    {
        var departments = await departmentRepository.GetAllAsync();
        var response = new DepartmentListResponse(
            departments.Select(d => new DepartmentDto(d.Id, d.Name, d.Code)).ToList()
        );
        return Result<DepartmentListResponse>.Success(response);
    }

    public async Task<Result<ClassListResponse>> GetClassesAsync(long? departmentId, int? fiscalYear, int? grade)
    {
        var classes = await classRepository.GetFilteredAsync(departmentId, fiscalYear, grade);
        
        var response = new ClassListResponse(
            classes.Select(c => new ClassDto(
                c.Id, 
                c.DepartmentId, 
                c.Department?.Name ?? "Unknown",
                c.FiscalYear,
                c.Grade,
                c.Name)).ToList()
        );
        return Result<ClassListResponse>.Success(response);
    }

    public async Task<Result<StudentProfileCreatedResponse>> CreateStudentProfileAsync(long accountId, CreateStudentProfileOnboardingRequest request)
    {
        // 1. アカウント情報の取得（ドメインルール検証用）
        var account = await accountRepository.GetByIdAsync(accountId);
        if (account == null) 
            return Result<StudentProfileCreatedResponse>.Failure("Account not found.", HttpStatusCode.NotFound, "NOT_FOUND_ERROR");

        // 2. アカウントごとの重複チェック
        var existingByAccount = await studentRepository.GetByAccountIdAsync(accountId);
        if (existingByAccount != null) 
            return Result<StudentProfileCreatedResponse>.Failure("Profile already exists for this account.", HttpStatusCode.Conflict, "CONFLICT_ERROR");

        // 3. 学籍番号の重複チェック
        var existingByNumber = await studentRepository.GetByStudentNumberAsync(request.StudentNumber);
        if (existingByNumber != null) 
            return Result<StudentProfileCreatedResponse>.Failure("Student number already exists.", HttpStatusCode.Conflict, "CONFLICT_ERROR");

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
            ProfileData = MapStudentToEntity(request.ProfileData, request.Pr)
        };

        // 4. ドメインルールの検証
        try
        {
            student.Validate(account.Email);
        }
        catch (ArgumentException ex)
        {
            return Result<StudentProfileCreatedResponse>.Failure(ex.Message, HttpStatusCode.BadRequest, "DOMAIN_VALIDATION_ERROR");
        }

        await studentRepository.AddAsync(student);

        var response = new StudentProfileCreatedResponse(
            student.Id,
            student.AccountId,
            student.ClassId,
            student.StudentNumber,
            student.IsJobHunting);

        return Result<StudentProfileCreatedResponse>.Success(response, "Student profile created successfully.");
    }

    public async Task<Result<TeacherProfileCreatedResponse>> CreateTeacherProfileAsync(long accountId, CreateTeacherProfileOnboardingRequest request)
    {
        var existing = await teacherRepository.GetByAccountIdAsync(accountId);
        if (existing != null) 
            return Result<TeacherProfileCreatedResponse>.Failure("Profile already exists for this account.", HttpStatusCode.Conflict, "CONFLICT_ERROR");

        // 担当クラスの重複チェック
        if (request.AssignedClasses != null)
        {
            var distinctClassIds = request.AssignedClasses.Select(a => a.ClassId).Distinct().Count();
            if (distinctClassIds != request.AssignedClasses.Count)
            {
                return Result<TeacherProfileCreatedResponse>.Failure("Duplicate class assignments are not allowed.", HttpStatusCode.BadRequest, "BAD_REQUEST_ERROR");
            }
        }

        var teacher = new Teacher
        {
            AccountId = accountId,
            Name = request.Name,
            NameKana = request.NameKana,
            Title = request.Title,
            OfficeLocation = request.OfficeLocation,
            ProfileData = MapTeacherToEntity(request.ProfileData)
        };

        // 担当クラスの割り当て
        if (request.AssignedClasses != null)
        {
            foreach (var assignment in request.AssignedClasses)
            {
                teacher.ClassTeachers.Add(new ClassTeacher
                {
                    ClassId = assignment.ClassId,
                    Role = (ClassTeacherRole)assignment.Role
                });
            }
        }

        await teacherRepository.AddAsync(teacher);

        var response = new TeacherProfileCreatedResponse(teacher.Id, teacher.AccountId);
        return Result<TeacherProfileCreatedResponse>.Success(response, "Teacher profile created successfully.");
    }

    public async Task<Result<StudentMeResponse>> GetStudentMeAsync(long accountId)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) 
            return Result<StudentMeResponse>.NotFound("Student profile not found.");

        var classDto = new ClassDto(
            student.Class.Id,
            student.Class.DepartmentId,
            student.Class.Department.Name,
            student.Class.FiscalYear,
            student.Class.Grade,
            student.Class.Name);

        var response = new StudentMeResponse(
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
            MapStudentToDto(student.ProfileData));

        return Result<StudentMeResponse>.Success(response);
    }

    public async Task<Result<TeacherMeResponse>> GetTeacherMeAsync(long accountId)
    {
        var teacher = await teacherRepository.GetByAccountIdAsync(accountId);
        if (teacher == null) 
            return Result<TeacherMeResponse>.NotFound("Teacher profile not found.");

        var response = new TeacherMeResponse(
            teacher.Id,
            teacher.AccountId,
            teacher.Name,
            teacher.NameKana,
            teacher.Title,
            teacher.OfficeLocation,
            MapTeacherToDto(teacher.ProfileData));

        return Result<TeacherMeResponse>.Success(response);
    }

    public async Task<Result> UpdateStudentProfileAsync(long accountId, UpdateStudentProfileRequest request)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) 
            return Result.NotFound("Student profile not found.");

        student.ProfileData = MapStudentToEntity(request.ProfileData, request.Pr, request.Certifications, request.Links);

        await studentRepository.UpdateAsync(student);
        return Result.Success("Student profile updated successfully.");
    }

    public async Task<Result> UpdateJobHuntingStatusAsync(long accountId, UpdateJobHuntingStatusRequest request)
    {
        var student = await studentRepository.GetByAccountIdAsync(accountId);
        if (student == null) 
            return Result.NotFound("Student profile not found.");

        student.IsJobHunting = request.IsJobHunting;

        await studentRepository.UpdateAsync(student);
        return Result.Success("Job hunting status updated successfully.");
    }

    public async Task<Result> UpdateTeacherProfileAsync(long accountId, UpdateTeacherProfileRequest request)
    {
        var teacher = await teacherRepository.GetByAccountIdAsync(accountId);
        if (teacher == null) 
            return Result.NotFound("Teacher profile not found.");

        teacher.Title = request.Title;
        teacher.OfficeLocation = request.OfficeLocation;

        teacher.ProfileData = MapTeacherToEntity(request.ProfileData, request.Career, request.Speciality);

        await teacherRepository.UpdateAsync(teacher);
        return Result.Success("Teacher profile updated successfully.");
    }

    // --- Students Mapping ---

    private static StudentProfile MapStudentToEntity(StudentProfileDataDto? dto, string? pr = null, string? certs = null, string? links = null)
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

    private static StudentProfileDataDto? MapStudentToDto(StudentProfile? entity)
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

    // --- Teachers Mapping ---

    private static TeacherProfile MapTeacherToEntity(TeacherProfileDataDto? dto, string? career = null, string? speciality = null)
    {
        var entity = new TeacherProfile
        {
            Career = career,
            Speciality = speciality
        };

        if (dto == null) return entity;

        if (dto.CareerHistory != null)
        {
            entity.CareerHistory = new CareerInfo
            {
                Summary = dto.CareerHistory.Summary,
                Details = dto.CareerHistory.Details?.Select(d => new CareerDetail
                {
                    Period = d.Period,
                    Organization = d.Organization,
                    Content = d.Content
                }).ToList()
            };
        }

        entity.SpecialityDetails = dto.SpecialityDetails?.Select(s => new SpecialityDetail
        {
            Name = s.Name,
            Description = s.Description
        }).ToList();

        if (dto.Consultation != null)
        {
            entity.Consultation = new ConsultationInfo
            {
                Style = dto.Consultation.Style,
                AvailableTopics = dto.Consultation.AvailableTopics,
                OfficeHours = dto.Consultation.OfficeHours
            };
        }

        entity.Message = dto.Message;

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

        return entity;
    }

    private static TeacherProfileDataDto? MapTeacherToDto(TeacherProfile? entity)
    {
        if (entity == null) return null;

        return new TeacherProfileDataDto(
            CareerHistory: entity.CareerHistory != null ? new CareerInfoDto(
                entity.CareerHistory.Summary,
                entity.CareerHistory.Details?.Select(d => new CareerDetailDto(d.Period, d.Organization, d.Content)).ToList()
            ) : null,
            SpecialityDetails: entity.SpecialityDetails?.Select(s => new SpecialityDetailDto(s.Name, s.Description)).ToList(),
            Consultation: entity.Consultation != null ? new ConsultationInfoDto(
                entity.Consultation.Style,
                entity.Consultation.AvailableTopics,
                entity.Consultation.OfficeHours
            ) : null,
            Message: entity.Message,
            SocialLinks: entity.SocialLinks != null ? new SocialLinksDto(
                entity.SocialLinks.Github,
                entity.SocialLinks.Portfolio,
                entity.SocialLinks.Blog,
                entity.SocialLinks.Twitter
            ) : null
        );
    }
}
