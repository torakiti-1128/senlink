namespace SenLink.Service.Modules.School.DTOs;

public record CreateStudentProfileOnboardingRequest(
    long ClassId,
    string StudentNumber,
    string Name,
    string NameKana,
    DateTime DateOfBirth,
    short Gender,
    int AdmissionYear,
    bool IsJobHunting = true,
    string? Pr = null,
    StudentProfileDataDto? ProfileData = null);

public record CreateTeacherProfileOnboardingRequest(
    string Name,
    string NameKana,
    string? Title,
    string? OfficeLocation,
    object? ProfileData);

public record UpdateStudentProfileRequest(
    string? Pr,
    string? Certifications,
    string? Links,
    StudentProfileDataDto? ProfileData = null);

public record UpdateJobHuntingStatusRequest(
    bool IsJobHunting);

public record UpdateTeacherProfileRequest(
    string? Title,
    string? OfficeLocation,
    string? Career,
    string? Speciality);

// ProfileData の詳細構造
public record StudentProfileDataDto(
    List<AcademicHistoryDto>? AcademicHistories = null,
    List<WorkHistoryDto>? WorkHistories = null,
    List<CertificationDetailDto>? CertificationDetails = null,
    SkillSetDto? Skills = null,
    SocialLinksDto? SocialLinks = null,
    SelfPromotionDetailDto? SelfPromotion = null
);

public record AcademicHistoryDto(
    string SchoolName,
    string? Faculty,
    string StartDate,
    string? EndDate,
    string Status);

public record WorkHistoryDto(
    string Type,
    string Organization,
    string? Role,
    string? Content,
    string StartDate,
    string? EndDate);

public record CertificationDetailDto(
    string Name,
    string? Date);

public record SkillSetDto(
    List<string>? Languages,
    List<string>? Frameworks,
    List<string>? Others);

public record SocialLinksDto(
    string? Github,
    string? Portfolio,
    string? Blog,
    string? Twitter);

public record SelfPromotionDetailDto(
    string? Catchphrase,
    string? Content,
    List<string>? Strengths);
