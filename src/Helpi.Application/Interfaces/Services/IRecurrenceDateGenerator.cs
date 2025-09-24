
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces.Services;

public interface IRecurrenceDateGenerator
{
    public IEnumerable<DateOnly> GetDates(
        DateOnly startDate,
        DateOnly? endDate,
        RecurrencePattern? pattern,
        DayOfWeek dayOfWeek,
        int horizonMonths = 3);
}