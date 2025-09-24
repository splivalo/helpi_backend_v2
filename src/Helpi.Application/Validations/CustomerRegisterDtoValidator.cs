using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using Helpi.Application.DTOs.Auth;
using Helpi.Domain.Enums;
namespace Helpi.Application.Validators;

public class CustomerRegisterDtoValidator : AbstractValidator<CustomerRegisterDto>
{
    public CustomerRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.UserType)
         .Equal(UserType.Customer)
         .WithMessage("UserType must be Customer for customer registration.");

        RuleFor(x => x.Relationship)
        .NotNull()
        .WithMessage("Relationship is required.")
        .Must(r => Enum.IsDefined(typeof(Relationship), r))
        .WithMessage("Relationship value is not valid.");

        RuleFor(x => x.PreferredNotificationMethod)
            .IsInEnum();

        RuleFor(x => x.ContactInfo)
            .SetValidator(new ContactInfoCreateDtoValidator());

        /// if is ordering for another
        RuleFor(x => x.SeniorContactInfo)
            .NotNull()
            .WithMessage("Senior contact info is required when ordering for another person.")
            .When(x => x.Relationship != Relationship.Self);

        RuleFor(x => x.SeniorContactInfo!)
            .SetValidator(new ContactInfoCreateDtoValidator())
            .When(x => x.Relationship != Relationship.Self && x.SeniorContactInfo != null);
    }
}
