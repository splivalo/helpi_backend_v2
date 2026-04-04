using FluentValidation;
using Helpi.Application.DTOs;

namespace Helpi.Application.Validators;

public class PricingConfigurationDtoValidator : AbstractValidator<PricingConfigurationDto>
{
    public PricingConfigurationDtoValidator()
    {
        RuleFor(x => x.JobHourlyRate)
            .GreaterThan(0).WithMessage("Job hourly rate must be greater than zero.");

        RuleFor(x => x.SundayHourlyRate)
            .GreaterThan(0).WithMessage("Sunday hourly rate must be greater than zero.");

        RuleFor(x => x.StudentHourlyRate)
            .GreaterThan(0).WithMessage("Student hourly rate must be greater than zero.");

        RuleFor(x => x.StudentSundayHourlyRate)
            .GreaterThan(0).WithMessage("Student Sunday hourly rate must be greater than zero.");

        RuleFor(x => x.IntermediaryPercentage)
            .InclusiveBetween(0, 100).WithMessage("Intermediary percentage must be between 0 and 100.");

        RuleFor(x => x.StudentCancelCutoffHours)
            .InclusiveBetween(1, 48).WithMessage("Student cancel cutoff must be between 1 and 48 hours.");

        RuleFor(x => x.SeniorCancelCutoffHours)
            .InclusiveBetween(1, 48).WithMessage("Senior cancel cutoff must be between 1 and 48 hours.");

        RuleFor(x => x.TravelBufferMinutes)
            .InclusiveBetween(0, 60).WithMessage("Travel buffer must be between 0 and 60 minutes.");

        RuleFor(x => x.PaymentTimingMinutes)
            .InclusiveBetween(5, 120).WithMessage("Payment timing must be between 5 and 120 minutes.");

        RuleFor(x => x.VatPercentage)
            .InclusiveBetween(0, 100).WithMessage("VAT percentage must be between 0 and 100.")
            .When(x => x.VatEnabled);
    }
}
