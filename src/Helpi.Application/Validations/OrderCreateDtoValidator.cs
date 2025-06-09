using FluentValidation;
using Helpi.Application.DTOs.Order;
namespace Helpi.Application.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(o => o.SeniorId)
            .GreaterThan(0)
            .WithMessage("SeniorId must be a valid positive integer.");

        RuleFor(o => o.StartDate)
            .Must(StartDate => StartDate.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow)
            .WithMessage("Order StartDate must be greater than today");

        RuleFor(o => o.EndDate)
            .Must((o, EndDate) => EndDate >= o.StartDate)
            .WithMessage("Order EndDate must be greater than or equal to order StartDate");

        RuleFor(o => o)
           .Must(o => !o.IsRecurring || o.EndDate > o.StartDate)
           .WithMessage("For recurring orders, EndDate must be after StartDate.");

        RuleFor(o => o)
          .Must(o => !o.IsRecurring || (o.EndDate.DayNumber - o.StartDate.DayNumber) >= 7)
          .WithMessage("For recurring orders, EndDate must be at least 7 days after StartDate.");

        // RuleFor(o => o.PaymentMethodId)
        // .Cascade(CascadeMode.Stop)
        // .NotNull().WithMessage("Payment method is required")
        // .GreaterThan(0).WithMessage("Invalid payment method");

        RuleFor(o => o)
            .Must(o => o.IsRecurring == (o.RecurrencePattern != null))
            .WithMessage("If the order is recurring, a recurrence pattern is required. Otherwise, it must be null.");



        RuleFor(o => o.Schedules)
            .Must(schedules => schedules.All(schedule =>
            {
                TimeOnly start = schedule.StartTime;
                TimeOnly end = schedule.EndTime;

                // Compare if the end time is greater than the start time
                return end > start;
            }))
            .WithMessage("End time must be greater than start time for all schedules.");

        RuleFor(o => o.Schedules)
            .Must(schedules => schedules.All(schedule =>
                schedule.DayOfWeek >= 1 && schedule.DayOfWeek <= 7))
            .WithMessage("Schedule dayOfWeek must be between 1 and 7.");
    }
}