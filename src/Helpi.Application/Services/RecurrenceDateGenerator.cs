using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class RecurrenceDateGenerator : IRecurrenceDateGenerator


{
    public IEnumerable<DateOnly> GetDates(
    DateOnly startDate,
    DateOnly? endDate,
    RecurrencePattern? pattern,
    DayOfWeek dayOfWeek,
    int horizonMonths = 3)
    {
        var dates = new List<DateOnly>();
        var current = startDate;

        /// if not reccurring
        if (pattern == null)
        {
            dates.Add(startDate);
            return dates;
        }

        // Calculate horizon date
        var maxHorizonDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(horizonMonths));
        var effectiveEndDate = endDate.HasValue
            ? (endDate.Value < maxHorizonDate ? endDate.Value : maxHorizonDate)
            : maxHorizonDate;


        while (current <= effectiveEndDate)
        {

            switch (pattern)
            {
                case RecurrencePattern.Weekly when current.DayOfWeek == dayOfWeek:
                    dates.Add(current);
                    current = current.AddDays(7);
                    break;

            }
        }

        return dates;
    }




}