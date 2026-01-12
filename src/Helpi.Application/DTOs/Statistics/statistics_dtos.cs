namespace Helpi.Application.DTOs;

public class StatisticsResponse
{
    public List<WeekStatistics> Weeks { get; set; } = new();
    public List<MonthStatistics> Months { get; set; } = new();
    public List<YearStatistics> Years { get; set; } = new();
}

public class WeekStatistics
{
    public int Index { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public List<double> Hours { get; set; } = new();
    public double WeekTotal { get; set; }
    public double? WeekPercentageChange { get; set; }
}

public class MonthStatistics
{
    public int Index { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public List<double> Hours { get; set; } = new();
    public double MonthTotal { get; set; }
    public double? MonthPercentageChange { get; set; }
}

public class YearStatistics
{
    public int Index { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public List<double> Hours { get; set; } = new();
    public double YearTotal { get; set; }
    public double? YearPercentageChange { get; set; }
}