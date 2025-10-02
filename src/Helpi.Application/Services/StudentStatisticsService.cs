

using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Application.Services;

public class StudentStatisticsService : IStudentStatisticsService
{
    private readonly IJobInstanceRepository _jobInstanceRepository;

    public StudentStatisticsService(IJobInstanceRepository jobInstanceRepository)
    {
        _jobInstanceRepository = jobInstanceRepository;
    }

    public async Task<StatisticsResponse> GetStudentStatisticsAsync(int studentId, int weeksBack, int monthsBack, int yearsBack)
    {
        var response = new StatisticsResponse
        {
            Weeks = await GetWeeklyStatisticsAsync(studentId, weeksBack),
            Months = await GetMonthlyStatisticsAsync(studentId, monthsBack),
            Years = await GetYearlyStatisticsAsync(studentId, yearsBack)
        };

        return response;
    }

    private async Task<List<WeekStatistics>> GetWeeklyStatisticsAsync(int studentId, int weeksBack)
    {
        var weeklyStats = new List<WeekStatistics>();
        var today = DateTime.Today;

        for (int i = weeksBack - 1; i >= 0; i--)
        {
            var weekStart = today.AddDays(-(int)today.DayOfWeek - (i * 7) + 1); // Monday
            var weekEnd = weekStart.AddDays(6); // Sunday

            var weekData = await GetWeekDataAsync(studentId, weekStart, weekEnd, i);
            weeklyStats.Add(weekData);
        }

        CalculatePercentageChanges(weeklyStats);
        return weeklyStats;
    }

    private async Task<WeekStatistics> GetWeekDataAsync(int studentId, DateTime weekStart, DateTime weekEnd, int index)
    {
        var jobs = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(
            studentId, weekStart, weekEnd);

        var dailyHours = new List<double>();
        for (int day = 0; day < 7; day++)
        {
            var currentDay = weekStart.AddDays(day);
            var dayHours = jobs
                .Where(j => j.ScheduledDate.ToDateTime(TimeOnly.MinValue) == currentDay)
                .Sum(j => (double)j.DurationHours);
            dailyHours.Add(dayHours);
        }

        return new WeekStatistics
        {
            Index = index,
            FromDate = DateOnly.FromDateTime(weekStart),
            ToDate = DateOnly.FromDateTime(weekEnd),
            Hours = dailyHours,
            WeekTotal = dailyHours.Sum()
        };
    }

    private async Task<List<MonthStatistics>> GetMonthlyStatisticsAsync(int studentId, int monthsBack)
    {
        var monthlyStats = new List<MonthStatistics>();
        var today = DateTime.Today;

        for (int i = monthsBack - 1; i >= 0; i--)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthData = await GetMonthDataAsync(studentId, monthStart, monthEnd, i);
            monthlyStats.Add(monthData);
        }

        CalculatePercentageChanges(monthlyStats);
        return monthlyStats;
    }

    private async Task<MonthStatistics> GetMonthDataAsync(int studentId, DateTime monthStart, DateTime monthEnd, int index)
    {
        var jobs = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(
            studentId, monthStart, monthEnd);

        // Group by week for monthly breakdown
        var weeklyHours = new List<double>();
        var currentWeekStart = monthStart;

        while (currentWeekStart <= monthEnd)
        {
            var weekEnd = currentWeekStart.AddDays(6) > monthEnd ? monthEnd : currentWeekStart.AddDays(6);
            var weekHours = jobs
                .Where(j => j.ScheduledDate.ToDateTime(TimeOnly.MinValue) >= currentWeekStart &&
                           j.ScheduledDate.ToDateTime(TimeOnly.MinValue) <= weekEnd)
                .Sum(j => (double)j.DurationHours);

            weeklyHours.Add(weekHours);
            currentWeekStart = currentWeekStart.AddDays(7);
        }

        return new MonthStatistics
        {
            Index = index,
            FromDate = DateOnly.FromDateTime(monthStart),
            ToDate = DateOnly.FromDateTime(monthEnd),
            Hours = weeklyHours,
            MonthTotal = weeklyHours.Sum()
        };
    }

    private async Task<List<YearStatistics>> GetYearlyStatisticsAsync(int studentId, int yearsBack)
    {
        var yearlyStats = new List<YearStatistics>();
        var today = DateTime.Today;

        for (int i = yearsBack - 1; i >= 0; i--)
        {
            var yearStart = new DateTime(today.Year - i, 1, 1);
            var yearEnd = new DateTime(today.Year - i, 12, 31);

            var yearData = await GetYearDataAsync(studentId, yearStart, yearEnd, i);
            yearlyStats.Add(yearData);
        }

        CalculatePercentageChanges(yearlyStats);
        return yearlyStats;
    }

    private async Task<YearStatistics> GetYearDataAsync(int studentId, DateTime yearStart, DateTime yearEnd, int index)
    {
        var jobs = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(
            studentId, yearStart, yearEnd);

        var monthlyHours = new List<double>();

        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(yearStart.Year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthHours = jobs
                .Where(j => j.ScheduledDate.ToDateTime(TimeOnly.MinValue) >= monthStart &&
                           j.ScheduledDate.ToDateTime(TimeOnly.MinValue) <= monthEnd)
                .Sum(j => (double)j.DurationHours);

            monthlyHours.Add(monthHours);
        }

        return new YearStatistics
        {
            Index = index,
            FromDate = DateOnly.FromDateTime(yearStart),
            ToDate = DateOnly.FromDateTime(yearEnd),
            Hours = monthlyHours,
            YearTotal = monthlyHours.Sum()
        };
    }

    private void CalculatePercentageChanges<T>(List<T> stats) where T : class
    {
        for (int i = 1; i < stats.Count; i++)
        {
            double? previousTotal = null;
            double currentTotal = 0;

            if (stats[i] is WeekStatistics currentWeek && stats[i - 1] is WeekStatistics previousWeek)
            {
                currentTotal = currentWeek.WeekTotal;
                previousTotal = previousWeek.WeekTotal;
                currentWeek.WeekPercentageChange = CalculatePercentageChange(currentTotal, previousTotal);
            }
            else if (stats[i] is MonthStatistics currentMonth && stats[i - 1] is MonthStatistics previousMonth)
            {
                currentTotal = currentMonth.MonthTotal;
                previousTotal = previousMonth.MonthTotal;
                currentMonth.MonthPercentageChange = CalculatePercentageChange(currentTotal, previousTotal);
            }
            else if (stats[i] is YearStatistics currentYear && stats[i - 1] is YearStatistics previousYear)
            {
                currentTotal = currentYear.YearTotal;
                previousTotal = previousYear.YearTotal;
                currentYear.YearPercentageChange = CalculatePercentageChange(currentTotal, previousTotal);
            }
        }
    }

    private double? CalculatePercentageChange(double current, double? previous)
    {
        if (previous == null || previous == 0) return null;
        return Math.Round(((current - previous.Value) / previous.Value) * 100, 2);
    }
}