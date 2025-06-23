
using FluentValidation;
using Helpi.Application.DTOs.Order;

namespace Helpi.Application.Validators;

public class OrderScheduleCreateDtoValidator : AbstractValidator<OrderScheduleCreateDto>
{

    public OrderScheduleCreateDtoValidator()
    {
        RuleFor(os => os)
            .Must(os =>
            {
                TimeOnly start = os.StartTime;
                TimeOnly end = os.EndTime;

                // Compare if the end time is greater than the start time
                return end > start;
            })
            .WithMessage("End time must be greater than start time for all schedules.");

        RuleFor(os => os)
            .Must(os => os.DayOfWeek >= 1 && os.DayOfWeek <= 7)
            .WithMessage("Schedule dayOfWeek must be between 1 and 7.");

    }


}