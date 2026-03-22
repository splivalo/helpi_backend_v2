namespace Helpi.Application.Utilities;

/// <summary>
/// Croatian public holidays — 13 fixed + 2 Easter-based (Easter Monday, Corpus Christi).
/// </summary>
public static class CroatianHolidays
{
    /// <summary>Returns true if the given date is a Croatian public holiday.</summary>
    public static bool IsPublicHoliday(DateOnly date)
    {
        var (month, day) = (date.Month, date.Day);

        // 13 fixed holidays
        if ((month, day) is
            (1, 1) or   // Nova godina
            (1, 6) or   // Sveta tri kralja
            (5, 1) or   // Praznik rada
            (5, 30) or  // Dan državnosti
            (6, 22) or  // Dan antifašističke borbe
            (8, 5) or   // Dan pobjede
            (8, 15) or  // Velika Gospa
            (10, 8) or  // Dan neovisnosti
            (11, 1) or  // Svi sveti
            (11, 18) or // Dan sjećanja na Vukovar
            (12, 25) or // Božić
            (12, 26))   // Sveti Stjepan
        {
            return true;
        }

        // Easter-based holidays
        var easter = ComputeEasterSunday(date.Year);
        var easterMonday = easter.AddDays(1);
        var corpusChristi = easter.AddDays(60);

        return date == easterMonday || date == corpusChristi;
    }

    /// <summary>
    /// Anonymous Gregorian algorithm (Computus) — calculates Easter Sunday for a given year.
    /// </summary>
    private static DateOnly ComputeEasterSunday(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateOnly(year, month, day);
    }
}
