namespace Helpi.Application.Common.Extensions;

public static class DayOfWeekExtensions
{
    /// <summary>
    /// Converts .NET DayOfWeek (Sunday = 0) to ISO-8601 weekday (Monday = 1 ... Sunday = 7)
    /// </summary>
    public static int ToIsoWeekday(this DayOfWeek day)
    {
        return day == DayOfWeek.Sunday ? 7 : (int)day;
    }

    /// <summary>
    /// Converts ISO-8601 weekday (1–7) back to .NET's DayOfWeek enum.
    /// </summary>
    public static DayOfWeek FromIsoWeekday(int isoDay)
    {
        return isoDay == 7 ? DayOfWeek.Sunday : (DayOfWeek)isoDay;
    }
}
