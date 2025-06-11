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

    public DashboardService(
        IOrderRepository orderRepository,
        IStudentContractRepository contractRepository,
        IStudentRepository studentRepository,
        IReviewRepository reviewRepository,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IJobInstanceRepository jobInstanceRepository,
        IScheduleAssignmentRepository assignmentRepository)
    {
        _orderRepository = orderRepository;
        _contractRepository = contractRepository;
        _studentRepository = studentRepository;
        _reviewRepository = reviewRepository;
        _customerRepository = customerRepository;
        _userRepository = userRepository;
        _jobInstanceRepository = jobInstanceRepository;
        _assignmentRepository = assignmentRepository;
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
            OrderStatus.Active,
            OrderStatus.Pending
        };

        // Count all "live" orders that have at least one active schedule
        // where none of the assignments have been accepted (i.e. still uncovered)
        var uncoveredOrders = await _orderRepository.CountAsync(order =>
            liveOrderStatuses.Contains(order.Status) &&
            order.Schedules.Any(schedule =>
                !schedule.IsCancelled &&
                schedule.Assignments.All(assignment => assignment.Status != AssignmentStatus.Accepted)
            )
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
        var expiredContracts = await _contractRepository.CountAsync(c => today > c.ExpirationDate);
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
        StudentStatus.Verified,
        StudentStatus.ContractRenewalNeeded
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
        // Students without valid contracts (expired or missing)
        // var validStudentIds = await _contractRepository.GetValidContractStudentIdsAsync();

        var current = 0;
        // await _studentRepository.CountAsync(s =>
        //     s.VerificationStatus == VerificationStatus.Verified &&
        //     !validStudentIds.Contains(s.UserId));

        // var previousValidIds = await _contractRepository.GetValidContractStudentIdsAsync(lastMonthEnd);
        var previous = 0;
        //  await _studentRepository.CountAsync(s =>
        //     s.VerificationStatus == VerificationStatus.Verified &&
        //     s.DateRegistered <= lastMonthEnd &&
        //     !previousValidIds.Contains(s.UserId));

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
     j => (int)(j.StartTime - j.EndTime).TotalHours
 );

        var previousHours = await _jobInstanceRepository.SumAsync(
            j => j.Status == JobInstanceStatus.Completed &&
                 j.ScheduledDate >= prevStartDate &&
                 j.ScheduledDate <= prevEndDate,
            j => (int)(j.StartTime - j.EndTime).TotalHours
        );


        return CreateTileData(
            DashboardTileType.workedHours,
            currentHours,
            previousHours,
            changeType: DetermineChangeType(currentHours, previousHours)
        );
    }


}