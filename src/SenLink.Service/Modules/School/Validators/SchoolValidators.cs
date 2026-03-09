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
            .Matches(@"^\d{7}$").WithMessage("Student number must be 7 digits.");

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
