using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.Validations;

public class ValidScheduleAttribute : ValidationAttribute
{

    /// TODO:
    // protected override ValidationResult IsValid(object value, ValidationContext context)
    // {
    //     var schedules = value as List<ScheduleRequest>;
    //     if (schedules == null) return ValidationResult.Success;

    //     foreach (var schedule in schedules)
    //     {
    //         if (schedule.EndTime <= schedule.StartTime)
    //         {
    //             return new ValidationResult("End time must be after start time");
    //         }
    //     }

    //     return ValidationResult.Success;
    // }
}