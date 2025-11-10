using FluentValidation;

using Helpi.Application.DTOs;

namespace Helpi.Application.Validators;

public class ContactInfoCreateDtoValidator : AbstractValidator<ContactInfoCreateDto>
{
    public ContactInfoCreateDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .MaximumLength(2);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-]{7,20}$")
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Gender value is not valid.");

        RuleFor(x => x.GooglePlaceId)
            .NotEmpty();

        RuleFor(x => x.FullAddress)
            .NotEmpty();
    }
}
