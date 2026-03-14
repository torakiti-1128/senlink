namespace SenLink.Service.Modules.School.DTOs;

public record DepartmentDto(long DepartmentId, string Name, string Code);

public record DepartmentListResponse(List<DepartmentDto> Items);

public record ClassDto(
    long ClassId, 
    long DepartmentId, 
    string DepartmentName, 
    int FiscalYear, 
    int Grade, 
    string Name);

public record ClassListResponse(List<ClassDto> Items);

public record StudentProfileCreatedResponse(
    long StudentId,
    long AccountId,
    long ClassId,
    string StudentNumber,
    bool IsJobHunting);

public record TeacherProfileCreatedResponse(
    long TeacherId,
    long AccountId);

public record StudentMeResponse(
    long StudentId,
    long AccountId,
    string StudentNumber,
    string Name,
    string NameKana,
    ClassDto Class,
    DateTime DateOfBirth,
    short Gender,
    int AdmissionYear,
    bool IsJobHunting,
    StudentProfileDataDto? ProfileData);

public record TeacherMeResponse(
    long TeacherId,
    long AccountId,
    string Name,
    string NameKana,
    string? Title,
    string? OfficeLocation,
    TeacherProfileDataDto? ProfileData);
