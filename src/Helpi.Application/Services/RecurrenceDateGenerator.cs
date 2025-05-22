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
        DayOfWeek dayOfWeek,
        int horizonMonths = 3)
    {
        var dates = new List<DateOnly>();

        // If the recurrence pattern is null, return a single occurrence at startDate
        if (pattern == null)
        {
            dates.Add(startDate);
            return dates;
        }

        // Calculate the furthest date allowed (effectiveEndDate), which is the smaller of endDate or the horizon
        var maxHorizonDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(horizonMonths));
        var effectiveEndDate = endDate.HasValue && endDate.Value < maxHorizonDate
            ? endDate.Value
            : maxHorizonDate;

        // Align the current date to the first occurrence of the scheduled dayOfWeek after or on startDate
        var daysToAdd = ((int)dayOfWeek - (int)startDate.DayOfWeek + 7) % 7;
        var current = startDate.AddDays(daysToAdd);

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



}