using FluentValidation;
using SenLink.Service.Modules.School.DTOs;

namespace SenLink.Service.Modules.School.Validators;

/// <summary>
/// 学生プロフィール登録のバリデーター
/// </summary>
public class CreateStudentProfileValidator : AbstractValidator<CreateStudentProfileOnboardingRequest>
{
    public CreateStudentProfileValidator()
    {
        RuleFor(x => x.ClassId).GreaterThan(0);
        
        RuleFor(x => x.StudentNumber)
            .NotEmpty()
            .Matches(@"^\d{8}$").WithMessage("Student number must be 8 digits.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameKana).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.Now);
        RuleFor(x => x.AdmissionYear).InclusiveBetween(2000, 2100);
    }
}

/// <summary>
/// 教員プロフィール登録のバリデーター
/// </summary>
public class CreateTeacherProfileValidator : AbstractValidator<CreateTeacherProfileOnboardingRequest>
{
    public CreateTeacherProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameKana).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).MaximumLength(100);
        RuleFor(x => x.OfficeLocation).MaximumLength(100);
    }
}

/// <summary>
/// 学生プロフィール更新のバリデーター
/// </summary>
public class UpdateStudentProfileValidator : AbstractValidator<UpdateStudentProfileRequest>
{
    public UpdateStudentProfileValidator()
    {
        RuleFor(x => x.Pr).MaximumLength(2000);
        RuleFor(x => x.Certifications).MaximumLength(1000);
        RuleFor(x => x.Links).MaximumLength(1000);
    }
}

/// <summary>
/// 教員プロフィール更新のバリデーター
/// </summary>
public class UpdateTeacherProfileValidator : AbstractValidator<UpdateTeacherProfileRequest>
{
    public UpdateTeacherProfileValidator()
    {
        RuleFor(x => x.Title).MaximumLength(100);
        RuleFor(x => x.OfficeLocation).MaximumLength(100);
        RuleFor(x => x.Career).MaximumLength(2000);
        RuleFor(x => x.Speciality).MaximumLength(1000);
    }
}
