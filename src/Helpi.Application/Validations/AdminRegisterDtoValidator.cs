using FluentValidation;
using Helpi.Application.DTOs.Auth;
using Helpi.Domain.Enums;

namespace Helpi.Application.Validators;

public class AdminRegisterDtoValidator : AbstractValidator<AdminRegisterDto>
{
    public AdminRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.UserType)
            .Equal(UserType.Admin)
             .WithMessage("UserType must be Admin for admin registration.");

        RuleFor(x => x.ContactInfo)
            .SetValidator(new ContactInfoCreateDtoValidator());
    }
}
