using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;


namespace Helpi.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStudentContractRepository _contractRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IScheduleAssignmentRepository _assignmentRepository;
    private readonly ISeniorRepository _seniorRepository;

    public DashboardService(
        IOrderRepository orderRepository,
        IStudentContractRepository contractRepository,
        IStudentRepository studentRepository,
        IReviewRepository reviewRepository,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IJobInstanceRepository jobInstanceRepository,
        IScheduleAssignmentRepository assignmentRepository,
        ISeniorRepository seniorRepository)
    {
        _orderRepository = orderRepository;
        _contractRepository = contractRepository;
        _studentRepository = studentRepository;
        _reviewRepository = reviewRepository;
        _customerRepository = customerRepository;
        _userRepository = userRepository;
        _jobInstanceRepository = jobInstanceRepository;
        _assignmentRepository = assignmentRepository;
        _seniorRepository = seniorRepository;
    }

    public async Task<List<DashboardTileData>> GetDashboardDataAsync()
    {
        var now = DateTime.UtcNow;

        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);

        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        var results = new List<DashboardTileData>
        {
            // Sequentially await each tile builder method to avoid DbContext concurrency
            await GetUncoveredOrdersDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetExpiredContractsDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetPlaceholderTileData(DashboardTileType.newNotifications),
            await GetPlaceholderTileData(DashboardTileType.newMessages),
            await GetStudentCountDataAsync(lastMonthEnd),
            await GetInvalidContractsDataAsync(lastMonthEnd),
            await GetReviewCountDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetAverageReviewDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetUserCountDataAsync(lastMonthEnd),
            await GetOrderCountDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetCompletedOrdersDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd),
            await GetWorkedHoursDataAsync(currentMonthStart, currentMonthEnd, lastMonthStart, lastMonthEnd)
        };

        return results;
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

    private async Task<DashboardTileData> GetUncoveredOrdersDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {
        // Orders without any accepted assignment
        // var current = await _orderRepository.CountAsync(o =>
        //     o.Status == OrderStatus.Pending &&
        //     o.CreatedAt >= currentStart &&
        //     o.CreatedAt <= currentEnd &&
        //     !o.Schedules.Any(s => s.Assignments.Any(a => a.Status == AssignmentStatus.Accepted)));

        // var previous = await _orderRepository.CountAsync(o =>
        //     o.Status == OrderStatus.Pending &&
        //     o.CreatedAt >= prevStart &&
        //     o.CreatedAt <= prevEnd &&
        //     !o.Schedules.Any(s => s.Assignments.Any(a => a.Status == AssignmentStatus.Accepted)));




        var liveOrderStatuses = new[] {
            OrderStatus.Pending,
            OrderStatus.FullAssigned
        };

        // Count all "live" orders that have at least one active schedule
        // where none of the (schedule)assignments have been accepted (i.e. still uncovered)
        // var uncoveredOrders = await _orderRepository.CountAsync(order =>
        //     liveOrderStatuses.Contains(order.Status) &&
        //     order.Schedules.Any(schedule =>
        //         !schedule.IsCancelled &&
        //         schedule.Assignments.Where(a => a.IsJobInstanceSub == false).All(assignment =>
        //          assignment.Status != AssignmentStatus.Accepted)
        //     )
        // );

        var uncoveredOrders = await _orderRepository.CountAsync(order =>
            order.Status == OrderStatus.Pending
        );

        var current = uncoveredOrders;
        var previous = uncoveredOrders;

        return CreateTileData(
            DashboardTileType.uncoveredOrders,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetExpiredContractsDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {
        // var current = await _contractRepository.CountAsync(c =>
        //     c.ExpirationDate >= currentStart &&
        //     c.ExpirationDate <= currentEnd);

        // var previous =  await _contractRepository.CountAsync(c =>
        //     c.ExpirationDate >= prevStart &&
        //     c.ExpirationDate <= prevEnd);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredContracts = await _studentRepository.CountAsync(s => s.Status == StudentStatus.Expired);

        var current = expiredContracts;
        var previous = expiredContracts;

        return CreateTileData(
            DashboardTileType.expiredContracts,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private Task<DashboardTileData> GetPlaceholderTileData(DashboardTileType type)
    {
        return Task.FromResult(new DashboardTileData
        {
            Type = type,
            Value = 0,
            ChangeType = ChangeType.remained
        });
    }

    private async Task<DashboardTileData> GetStudentCountDataAsync(DateTime lastMonthEnd)
    {

        var activeStudentStatus = new[]{
        StudentStatus.InActive,
        StudentStatus.Active,
        StudentStatus.ContractAboutToExpire
    };

        var current = await _studentRepository.CountAsync(s =>
            activeStudentStatus.Contains(s.Status));

        var previous = await _studentRepository.CountAsync(s =>
            activeStudentStatus.Contains(s.Status) &&
            s.DateRegistered <= lastMonthEnd);

        return CreateTileData(
            DashboardTileType.studentCount,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetInvalidContractsDataAsync(DateTime lastMonthEnd)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var prevToday = DateOnly.FromDateTime(lastMonthEnd);

        // CURRENT: invalid contracts as of today
        var current = await _contractRepository.CountAsync(c =>
            c.DeletedOn == null &&
            c.ExpirationDate < today &&
            !c.Student.Contracts.Any(ac =>
                ac.DeletedOn == null &&
                ac.EffectiveDate <= today &&
                ac.ExpirationDate >= today
            )
        );

        // PREVIOUS: invalid contracts as of last month end
        var previous = await _contractRepository.CountAsync(c =>
            c.DeletedOn == null &&
            c.ExpirationDate < prevToday &&
            !c.Student.Contracts.Any(ac =>
                ac.DeletedOn == null &&
                ac.EffectiveDate <= prevToday &&
                ac.ExpirationDate >= prevToday
            )
        );

        return CreateTileData(
            DashboardTileType.invalidContracts,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }


    private async Task<DashboardTileData> GetReviewCountDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {
        var current = await _reviewRepository.CountAsync(r =>
            r.CreatedAt >= currentStart &&
            r.CreatedAt <= currentEnd);

        var previous = await _reviewRepository.CountAsync(r =>
            r.CreatedAt >= prevStart &&
            r.CreatedAt <= prevEnd);

        return CreateTileData(
            DashboardTileType.reviewCount,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetAverageReviewDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {
        // Calculate average rating for current period
        var currentAvg = await _reviewRepository.AverageAsync(
            r => r.CreatedAt >= currentStart && r.CreatedAt <= currentEnd,
            r => r.Rating);

        var previousAvg = await _reviewRepository.AverageAsync(
            r => r.CreatedAt >= prevStart && r.CreatedAt <= prevEnd,
            r => r.Rating);

        // Handle no reviews case
        var currentValue = currentAvg.HasValue ? currentAvg.Value : 0;
        var previousValue = previousAvg.HasValue ? previousAvg.Value : 0;

        return CreateTileData(
            DashboardTileType.averageReview,
            currentValue,
            previousValue,
            changeType: DetermineChangeType(currentValue, previousValue)
        );
    }

    private async Task<DashboardTileData> GetUserCountDataAsync(DateTime lastMonthEnd)
    {
        var current = await _customerRepository.CountAsync(c => true);

        var previous = await _customerRepository.CountAsync(u => u.CreatedAt <= lastMonthEnd);

        return CreateTileData(
            DashboardTileType.userCount,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetOrderCountDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {
        var current = await _orderRepository.CountAsync(o =>
            o.CreatedAt >= currentStart &&
            o.CreatedAt <= currentEnd);

        var previous = await _orderRepository.CountAsync(o =>
            o.CreatedAt >= prevStart &&
            o.CreatedAt <= prevEnd);

        return CreateTileData(
            DashboardTileType.orderCount,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetCompletedOrdersDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {
        var current = await _orderRepository.CountAsync(o =>
            o.Status == OrderStatus.Completed &&
            o.UpdatedAt >= currentStart &&
            o.UpdatedAt <= currentEnd);

        var previous = await _orderRepository.CountAsync(o =>
            o.Status == OrderStatus.Completed &&
            o.UpdatedAt >= prevStart &&
            o.UpdatedAt <= prevEnd);

        return CreateTileData(
            DashboardTileType.completedOrders,
            current,
            previous,
            changeType: DetermineChangeType(current, previous)
        );
    }

    private async Task<DashboardTileData> GetWorkedHoursDataAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime prevStart, DateTime prevEnd)
    {

        var currentStartDate = DateOnly.FromDateTime(currentStart);
        var currentEndDate = DateOnly.FromDateTime(currentEnd);
        var prevStartDate = DateOnly.FromDateTime(prevStart);
        var prevEndDate = DateOnly.FromDateTime(prevEnd);


        // Calculate total minutes worked
        var currentHours = await _jobInstanceRepository.SumAsync(
                        j => j.Status == JobInstanceStatus.Completed &&
                        j.ScheduledDate >= currentStartDate &&
                        j.ScheduledDate <= currentEndDate,
                        j => (int)(j.EndTime - j.StartTime).TotalHours);

        var previousHours = await _jobInstanceRepository.SumAsync(
            j => j.Status == JobInstanceStatus.Completed &&
                 j.ScheduledDate >= prevStartDate &&
                 j.ScheduledDate <= prevEndDate,
            j => (int)(j.EndTime - j.StartTime).TotalHours
        );


        return CreateTileData(
            DashboardTileType.workedHours,
            currentHours,
            previousHours,
            changeType: DetermineChangeType(currentHours, previousHours)
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