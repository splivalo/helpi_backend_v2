using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;
public class RecurrenceDateGenerator : IRecurrenceDateGenerator
{
    public IEnumerable<DateOnly> GetDates(
    DateOnly startDate,
    DateOnly? endDate,
    RecurrencePattern pattern,
    DayOfWeek dayOfWeek,
    int horizonMonths = 3)
    {
        var dates = new List<DateOnly>();
        var current = startDate;

        // Calculate horizon date
        var maxHorizonDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(horizonMonths));
        var effectiveEndDate = endDate.HasValue
            ? (endDate.Value < maxHorizonDate ? endDate.Value : maxHorizonDate)
            : maxHorizonDate;

        while (current <= effectiveEndDate)
        {
            // Existing pattern matching logic
            switch (pattern)
            {
                case RecurrencePattern.Daily:
                    dates.Add(current);
                    current = current.AddDays(1);
                    break;
                case RecurrencePattern.Weekly when current.DayOfWeek == dayOfWeek:
                    dates.Add(current);
                    current = current.AddDays(7);
                    break;
                case RecurrencePattern.Biweekly when current.DayOfWeek == dayOfWeek:
                    dates.Add(current);
                    current = current.AddDays(14);
                    break;
                case RecurrencePattern.Monthly:
                    dates.Add(current);
                    current = current.AddMonths(1);
                    break;
                default:
                    current = current.AddDays(1);
                    break;
            }
        }

        return dates;
    }



}