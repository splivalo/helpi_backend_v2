

namespace Helpi.Application.Utilities
{
    public static class DateTimeUtils
    {
        // Default timezone for Croatia
        private static readonly string DefaultTimeZoneId = "Europe/Zagreb";

        /// <summary>
        /// Converts DateOnly + TimeOnly to UTC DateTime using a specified timezone or default Croatia timezone.
        /// </summary>
        /// <param name="date">DateOnly part</param>
        /// <param name="time">TimeOnly part</param>
        /// <param name="timeZoneId">Optional timezone ID. Defaults to Europe/Zagreb</param>
        /// <returns>UTC DateTime</returns>
        public static DateTime ToUtc(this DateOnly date, TimeOnly time, string? timeZoneId = null)
        {
            // Combine DateOnly + TimeOnly to local DateTime
            var localDateTime = date.ToDateTime(time);

            // Get the timezone
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? DefaultTimeZoneId);

            // Convert to UTC
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
        }

        /// <summary>
        /// Converts UTC DateTime to DateTime in a specified timezone or default Croatia timezone.
        /// </summary>
        /// <param name="utcDateTime">UTC DateTime</param>
        /// <param name="timeZoneId">Optional timezone ID. Defaults to Europe/Zagreb</param>
        /// <returns>Local DateTime</returns>
        public static DateTime FromUtc(DateTime utcDateTime, string? timeZoneId = null)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId ?? DefaultTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
        }
    }
}
