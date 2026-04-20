using Helpi.Application.Common.Extensions;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class RecurrenceDateGenerator : IRecurrenceDateGenerator


{/// <summary>
 /// Generates a list of recurring dates based on the specified recurrence pattern and scheduled day of the week.
 /// </summary>
 /// <param name="startDate">The date from which to begin generating recurrence.</param>
 /// <param name="endDate">
 /// The optional end date for recurrence. If null, the recurrence will stop at the horizon limit (defaults to 3 months from now).
 /// </param>
 /// <param name="pattern">
 /// The recurrence pattern (e.g., Weekly). If null, a one-time occurrence is returned at the start date.
 /// </param>
 /// <param name="dayOfWeek">The day of the week on which the recurrence should occur (e.g., Monday).</param>
 /// <param name="horizonMonths">
 /// The number of months from today to limit recurrence generation. Used as a fallback if no end date is specified.
 /// </param>
 /// <returns>A list of DateOnly values that match the recurrence rule.</returns>
    public IEnumerable<DateOnly> GetDates(
        DateOnly startDate,
        DateOnly? endDate,
        RecurrencePattern? pattern,
        DayOfWeek isoDayOfWeek,
        int horizonMonths = 3,
        DateOnly? contractEndDate = null)
    {

        /// .net uses 0 - 6 ... WE store 1 -7
        var dayOfWeek = DayOfWeekExtensions.FromIsoWeekday((int)isoDayOfWeek);

        var dates = new List<DateOnly>();

        // Calculate the furthest date allowed (effectiveEndDate), which is the smallest of endDate, contractEndDate, or the horizon
        var maxHorizonDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(horizonMonths));
        var effectiveEndDate = maxHorizonDate;
        if (endDate.HasValue && endDate.Value < effectiveEndDate)
            effectiveEndDate = endDate.Value;
        if (contractEndDate.HasValue && contractEndDate.Value < effectiveEndDate)
            effectiveEndDate = contractEndDate.Value;

        // Align the current date to the first occurrence of the scheduled dayOfWeek after or on startDate
        var daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
        var current = startDate.AddDays(daysToAdd);

        // If no recurrence → return ONLY the first valid aligned date (if in range)
        if (pattern == null)
        {
            if (current <= effectiveEndDate)
                dates.Add(current);

            return dates;
        }

        // Generate dates according to the recurrence pattern
        while (current <= effectiveEndDate)
        {
            switch (pattern)
            {
                case RecurrencePattern.Weekly when current.DayOfWeek == dayOfWeek:
                    dates.Add(current);
                    current = current.AddDays(7); // Move to the next occurrence (next week)
                    break;

                // Future pattern cases (e.g., Monthly) can be added here

                default:
                    // If the pattern is unrecognized or not handled, break the loop
                    current = effectiveEndDate.AddDays(1); // Ensures loop exits
                    break;
            }
        }

        return dates;
    }


    // private DayOfWeek FromIsoDayNumber(int isoDay)
    // {
    //     if (isoDay < 1 || isoDay > 7)
    //         throw new ArgumentOutOfRangeException(nameof(isoDay), "Day of week must be between 1 (Monday) and 7 (Sunday).");

    //     // Adjust ISO 8601 day to .NET DayOfWeek:
    //     // ISO: 1 (Mon) → .NET 1, ..., 6 (Sat) → 6, 7 (Sun) → 0
    //     return (DayOfWeek)(isoDay % 7);
    // }


}