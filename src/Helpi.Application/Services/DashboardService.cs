using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Enums;


namespace Helpi.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStudentContractRepository _contractRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly ISeniorRepository _seniorRepository;

    public DashboardService(
        IOrderRepository orderRepository,
        IStudentContractRepository contractRepository,
        IStudentRepository studentRepository,
        IJobInstanceRepository jobInstanceRepository,
        ISeniorRepository seniorRepository)
    {
        _orderRepository = orderRepository;
        _contractRepository = contractRepository;
        _studentRepository = studentRepository;
        _jobInstanceRepository = jobInstanceRepository;
        _seniorRepository = seniorRepository;
    }

    public async Task<List<DashboardTileData>> GetAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        return new List<DashboardTileData>
        {
            await GetAdminProcessingOrdersAsync(lastMonthEnd),
            await GetAdminActiveOrdersAsync(lastMonthEnd),
            await GetAdminTotalStudentsAsync(lastMonthEnd),
            await GetAdminTotalSeniorsAsync(lastMonthEnd)
        };
    }


    private DashboardTileData CreateTileData(
        DashboardTileType type,
        double currentValue,
        double previousValue,
        double? percentage = null,
        ChangeType changeType = ChangeType.remained)
    {
        return new DashboardTileData
        {
            Type = type,
            Value = currentValue,
            Percentage = percentage ?? CalculatePercentageChange(currentValue, previousValue),
            ChangeType = changeType,
            Period = "last month"
        };
    }

    private double? CalculatePercentageChange(double current, double previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((current - previous) / (double)previous * 100, 2);
    }

    private ChangeType DetermineChangeType(double current, double previous)
    {
        return current > previous ? ChangeType.increased
             : current < previous ? ChangeType.decreased
             : ChangeType.remained;
    }

    private async Task<DashboardTileData> GetAdminProcessingOrdersAsync(DateTime lastMonthEnd)
    {
        var current = await _orderRepository.CountAsync(order =>
            order.Status == OrderStatus.Pending);

        var previous = await _orderRepository.CountAsync(order =>
            order.Status == OrderStatus.Pending &&
            order.CreatedAt <= lastMonthEnd);

        return CreateTileData(
            DashboardTileType.adminProcessingOrders,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetAdminActiveOrdersAsync(DateTime lastMonthEnd)
    {
        var current = await _orderRepository.CountAsync(order =>
            order.Status == OrderStatus.FullAssigned);

        var previous = await _orderRepository.CountAsync(order =>
            order.Status == OrderStatus.FullAssigned &&
            order.CreatedAt <= lastMonthEnd);

        return CreateTileData(
            DashboardTileType.adminActiveOrders,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetAdminTotalStudentsAsync(DateTime lastMonthEnd)
    {
        var current = await _studentRepository.CountAsync(_ => true);

        var previous = await _studentRepository.CountAsync(s =>
            s.DateRegistered <= lastMonthEnd);

        return CreateTileData(
            DashboardTileType.adminTotalStudents,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetAdminTotalSeniorsAsync(DateTime lastMonthEnd)
    {
        var seniors = await _seniorRepository.GetSeniorsAsync();
        var current = seniors.Count;
        var previous = seniors.Count(senior => senior.CreatedAt <= lastMonthEnd);

        return CreateTileData(
            DashboardTileType.adminTotalSeniors,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    // ===== Student Dashboard =====

    public async Task<List<DashboardTileData>> GetStudentDashboardAsync(int studentId)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        return new List<DashboardTileData>
        {
            await GetStudentUpcomingSessionsAsync(studentId),
            await GetStudentCompletedSessionsAsync(studentId, currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetStudentEarningsAsync(studentId, currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetStudentRatingAsync(studentId),
            await GetStudentContractDaysAsync(studentId),
            await GetStudentWorkedHoursAsync(studentId, currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd)
        };
    }

    private async Task<DashboardTileData> GetStudentUpcomingSessionsAsync(int studentId)
    {
        var upcoming = await _jobInstanceRepository.GetStudentUpComingJobInstances(studentId);
        return new DashboardTileData
        {
            Type = DashboardTileType.upcomingSessions,
            Value = upcoming.Count(),
            ChangeType = ChangeType.remained
        };
    }

    private async Task<DashboardTileData> GetStudentCompletedSessionsAsync(
        int studentId, DateTime currentStart, DateTime currentEnd, DateTime prevStart, DateTime prevEnd)
    {
        var currentCompleted = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(studentId, currentStart, currentEnd);
        var prevCompleted = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(studentId, prevStart, prevEnd);

        var current = (double)currentCompleted.Count();
        var previous = (double)prevCompleted.Count();

        return CreateTileData(DashboardTileType.completedSessionsStudent, current, previous,
            changeType: DetermineChangeType(current, previous));
    }

    private async Task<DashboardTileData> GetStudentEarningsAsync(
        int studentId, DateTime currentStart, DateTime currentEnd, DateTime prevStart, DateTime prevEnd)
    {
        var currentInstances = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(studentId, currentStart, currentEnd);
        var prevInstances = await _jobInstanceRepository.GetCompletedJobInstancesForStudentAsync(studentId, prevStart, prevEnd);

        var current = (double)currentInstances.Sum(j => j.ServiceProviderAmount);
        var previous = (double)prevInstances.Sum(j => j.ServiceProviderAmount);

        return CreateTileData(DashboardTileType.totalEarnings, current, previous,
            changeType: DetermineChangeType(current, previous));
    }

    private async Task<DashboardTileData> GetStudentRatingAsync(int studentId)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        return new DashboardTileData
        {
            Type = DashboardTileType.myRating,
            Value = (double)student.AverageRating,
            ChangeType = ChangeType.remained
        };
    }

    private async Task<DashboardTileData> GetStudentContractDaysAsync(int studentId)
    {
        var contracts = await _contractRepository.GetByStudentIdAsync(studentId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeContract = contracts
            .Where(c => c.DeletedOn == null && c.EffectiveDate <= today && c.ExpirationDate >= today)
            .OrderBy(c => c.ExpirationDate)
            .FirstOrDefault();

        var daysRemaining = activeContract != null
            ? (activeContract.ExpirationDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).TotalDays
            : 0;

        return new DashboardTileData
        {
            Type = DashboardTileType.contractDaysRemaining,
            Value = daysRemaining,
            ChangeType = ChangeType.remained
        };
    }

    private async Task<DashboardTileData> GetStudentWorkedHoursAsync(
        int studentId, DateTime currentStart, DateTime currentEnd, DateTime prevStart, DateTime prevEnd)
    {
        var currentHours = await _jobInstanceRepository.GetTotalCompletedHoursForPeriodAsync(studentId, currentStart, currentEnd);
        var prevHours = await _jobInstanceRepository.GetTotalCompletedHoursForPeriodAsync(studentId, prevStart, prevEnd);

        return CreateTileData(DashboardTileType.workedHoursStudent, (double)currentHours, (double)prevHours,
            changeType: DetermineChangeType((double)currentHours, (double)prevHours));
    }

    // ===== Senior Dashboard =====

    public async Task<List<DashboardTileData>> GetSeniorDashboardAsync(int seniorId)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        return new List<DashboardTileData>
        {
            await GetSeniorActiveOrdersAsync(seniorId),
            await GetSeniorCompletedSessionsAsync(seniorId, currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetSeniorTotalSpentAsync(seniorId, currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetSeniorRatingAsync(seniorId)
        };
    }

    private async Task<DashboardTileData> GetSeniorActiveOrdersAsync(int seniorId)
    {
        var activeStatuses = new[] { OrderStatus.Pending, OrderStatus.FullAssigned };
        var count = await _orderRepository.CountAsync(o => o.SeniorId == seniorId && activeStatuses.Contains(o.Status));

        return new DashboardTileData
        {
            Type = DashboardTileType.activeOrders,
            Value = count,
            ChangeType = ChangeType.remained
        };
    }

    private async Task<DashboardTileData> GetSeniorCompletedSessionsAsync(
        int seniorId, DateTime currentStart, DateTime currentEnd, DateTime prevStart, DateTime prevEnd)
    {
        var allCompleted = (await _jobInstanceRepository.GetSeniorCompletedJobInstances(seniorId)).ToList();

        var currentStartDate = DateOnly.FromDateTime(currentStart);
        var currentEndDate = DateOnly.FromDateTime(currentEnd);
        var prevStartDate = DateOnly.FromDateTime(prevStart);
        var prevEndDate = DateOnly.FromDateTime(prevEnd);

        var current = (double)allCompleted.Count(j => j.ScheduledDate >= currentStartDate && j.ScheduledDate <= currentEndDate);
        var previous = (double)allCompleted.Count(j => j.ScheduledDate >= prevStartDate && j.ScheduledDate <= prevEndDate);

        return CreateTileData(DashboardTileType.completedSessionsSenior, current, previous,
            changeType: DetermineChangeType(current, previous));
    }

    private async Task<DashboardTileData> GetSeniorTotalSpentAsync(
        int seniorId, DateTime currentStart, DateTime currentEnd, DateTime prevStart, DateTime prevEnd)
    {
        var allCompleted = (await _jobInstanceRepository.GetSeniorCompletedJobInstances(seniorId)).ToList();

        var currentStartDate = DateOnly.FromDateTime(currentStart);
        var currentEndDate = DateOnly.FromDateTime(currentEnd);
        var prevStartDate = DateOnly.FromDateTime(prevStart);
        var prevEndDate = DateOnly.FromDateTime(prevEnd);

        var current = (double)allCompleted.Where(j => j.ScheduledDate >= currentStartDate && j.ScheduledDate <= currentEndDate).Sum(j => j.TotalAmount);
        var previous = (double)allCompleted.Where(j => j.ScheduledDate >= prevStartDate && j.ScheduledDate <= prevEndDate).Sum(j => j.TotalAmount);

        return CreateTileData(DashboardTileType.totalSpent, current, previous,
            changeType: DetermineChangeType(current, previous));
    }

    private async Task<DashboardTileData> GetSeniorRatingAsync(int seniorId)
    {
        var senior = await _seniorRepository.GetByIdAsync(seniorId);
        return new DashboardTileData
        {
            Type = DashboardTileType.myRatingSenior,
            Value = senior != null ? (double)senior.AverageRating : 0,
            ChangeType = ChangeType.remained
        };
    }
}