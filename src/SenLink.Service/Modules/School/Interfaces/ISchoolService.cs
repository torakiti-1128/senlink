using SenLink.Service.Modules.School.DTOs;
using SenLink.Shared.Results;

namespace SenLink.Service.Modules.School.Interfaces;

public interface ISchoolService
{
    Task<Result<DepartmentListResponse>> GetDepartmentsAsync();
    Task<Result<ClassListResponse>> GetClassesAsync(long? departmentId, int? fiscalYear, int? grade);
    Task<Result<StudentProfileCreatedResponse>> CreateStudentProfileAsync(long accountId, CreateStudentProfileOnboardingRequest request);
    Task<Result<TeacherProfileCreatedResponse>> CreateTeacherProfileAsync(long accountId, CreateTeacherProfileOnboardingRequest request);
    Task<Result<StudentMeResponse>> GetStudentMeAsync(long accountId);
    Task<Result<TeacherMeResponse>> GetTeacherMeAsync(long accountId);
    Task<Result> UpdateStudentProfileAsync(long accountId, UpdateStudentProfileRequest request);
    Task<Result> UpdateJobHuntingStatusAsync(long accountId, UpdateJobHuntingStatusRequest request);
    Task<Result> UpdateTeacherProfileAsync(long accountId, UpdateTeacherProfileRequest request);
}
