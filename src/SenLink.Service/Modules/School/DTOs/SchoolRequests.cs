namespace SenLink.Service.Modules.School.DTOs;

public record CreateStudentProfileOnboardingRequest(
    long ClassId,
    string StudentNumber,
    string Name,
    string NameKana,
    DateTime DateOfBirth,
    short Gender,
    int AdmissionYear);

public record CreateTeacherProfileOnboardingRequest(
    string Name,
    string NameKana,
    string? Title,
    string? OfficeLocation,
    object? ProfileData);
