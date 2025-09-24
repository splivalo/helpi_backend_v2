using FluentValidation;
using Helpi.Application.DTOs.Auth;
using Helpi.Domain.Enums;
namespace Helpi.Application.Validators;

public class StudentRegisterDtoValidator : AbstractValidator<StudentRegisterDto>
{
    public StudentRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.UserType)
            .Equal(UserType.Student)
            .WithMessage("UserType must be Student for student registration.");

        RuleFor(x => x.StudentNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.StudentNumber));

        RuleFor(x => x.FacultyId)
            .GreaterThan(0)
            .When(x => x.FacultyId.HasValue);

        RuleFor(x => x.ContactInfo)
            .SetValidator(new ContactInfoCreateDtoValidator());
    }
}
