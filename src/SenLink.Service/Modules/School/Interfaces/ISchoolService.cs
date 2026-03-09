using SenLink.Service.Modules.School.DTOs;

namespace SenLink.Service.Modules.School.Interfaces;

public interface ISchoolService
{
    Task<DepartmentListResponse> GetDepartmentsAsync();
    Task<ClassListResponse> GetClassesAsync(long? departmentId, int? fiscalYear, int? grade);
    Task<StudentProfileCreatedResponse> CreateStudentProfileAsync(long accountId, CreateStudentProfileOnboardingRequest request);
    Task<TeacherProfileCreatedResponse> CreateTeacherProfileAsync(long accountId, CreateTeacherProfileOnboardingRequest request);
    Task<StudentMeResponse?> GetStudentMeAsync(long accountId);
    Task<TeacherMeResponse?> GetTeacherMeAsync(long accountId);
    Task<bool> UpdateStudentProfileAsync(long accountId, UpdateStudentProfileRequest request);
    Task<bool> UpdateJobHuntingStatusAsync(long accountId, UpdateJobHuntingStatusRequest request);
    Task<bool> UpdateTeacherProfileAsync(long accountId, UpdateTeacherProfileRequest request);
}
