using FluentValidation;
using Helpi.Application.DTOs;

namespace Helpi.Application.Validators;

public class PricingConfigurationDtoValidator : AbstractValidator<PricingConfigurationDto>
{
    public PricingConfigurationDtoValidator()
    {
        RuleFor(x => x.JobHourlyRate)
            .GreaterThan(0).WithMessage("Job hourly rate must be greater than zero.");

        RuleFor(x => x.CompanyPercentage)
            .InclusiveBetween(0, 100).WithMessage("Company percentage must be between 0 and 100.");

        RuleFor(x => x.ServiceProviderPercentage)
            .InclusiveBetween(0, 100).WithMessage("Service provider percentage must be between 0 and 100.");

        RuleFor(x => x)
            .Must(HaveValidSplit)
            .WithMessage("CompanyPercentage + ServiceProviderPercentage must equal 100.");
    }

    private bool HaveValidSplit(PricingConfigurationDto dto)
    {
        return dto.CompanyPercentage + dto.ServiceProviderPercentage == 100;
    }
}
