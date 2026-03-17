using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services.Maintenance;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class SuspensionService
{
    private readonly IUserRepository _userRepository;
    private readonly ISuspensionLogRepository _suspensionLogRepository;
    private readonly IMapper _mapper;
    private readonly IReassignmentService _reassignmentService;
    private readonly IOrderRepository _orderRepository;
    private readonly OrderCancellationHandler _orderCancellationHandler;
    private readonly ILogger<SuspensionService> _logger;

    public SuspensionService(
        IUserRepository userRepository,
        ISuspensionLogRepository suspensionLogRepository,
        IMapper mapper,
        IReassignmentService reassignmentService,
        IOrderRepository orderRepository,
        OrderCancellationHandler orderCancellationHandler,
        ILogger<SuspensionService> logger)
    {
        _userRepository = userRepository;
        _suspensionLogRepository = suspensionLogRepository;
        _mapper = mapper;
        _reassignmentService = reassignmentService;
        _orderRepository = orderRepository;
        _orderCancellationHandler = orderCancellationHandler;
        _logger = logger;
    }

    public async Task<UserSuspensionStatusDto> GetSuspensionStatusAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var logs = await _suspensionLogRepository.GetByUserIdAsync(userId);

        return new UserSuspensionStatusDto
        {
            IsSuspended = user.IsSuspended,
            SuspensionReason = user.SuspensionReason,
            SuspendedAt = user.SuspendedAt,
            SuspendedByAdminId = user.SuspendedByAdminId,
            SuspensionHistory = _mapper.Map<List<SuspensionLogDto>>(logs)
        };
    }

    public async Task<SuspensionLogDto> SuspendUserAsync(int userId, string reason, int adminId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user.IsSuspended)
            throw new InvalidOperationException("User is already suspended.");

        if (user.UserType == UserType.Admin)
            throw new InvalidOperationException("Cannot suspend an admin user.");

        _logger.LogInformation("🚫 Suspending user {UserId} (Type: {UserType}). Reason: {Reason}",
            userId, user.UserType, reason);

        // Auto-cancel based on user type
        if (user.UserType == UserType.Student)
        {
            _logger.LogInformation("🔄 Auto-reassigning all jobs for suspended student {UserId}", userId);
            await _reassignmentService.ReassignExpiredContractJobs(userId);
            _logger.LogInformation("✅ Jobs reassigned for suspended student {UserId}", userId);
        }
        else if (user.UserType == UserType.Customer)
        {
            // Customers can have seniors - cancel all orders for their seniors
            _logger.LogInformation("🔄 Auto-cancelling all orders for suspended customer {UserId}", userId);
            await CancelAllOrdersForCustomerAsync(userId);
            _logger.LogInformation("✅ Orders cancelled for suspended customer {UserId}", userId);
        }

        user.IsSuspended = true;
        user.SuspensionReason = reason;
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspendedByAdminId = adminId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var log = new SuspensionLog
        {
            UserId = userId,
            Action = SuspensionAction.Suspended,
            Reason = reason,
            AdminId = adminId
        };

        await _suspensionLogRepository.AddAsync(log);

        _logger.LogInformation("✅ User {UserId} suspended successfully", userId);

        return _mapper.Map<SuspensionLogDto>(log);
    }

    /// <summary>
    /// Cancels all active orders for a customer (through their seniors).
    /// </summary>
    private async Task CancelAllOrdersForCustomerAsync(int customerId)
    {
        // Get all orders and filter by customer
        var orders = await _orderRepository.GetAllAsync(null);

        var customerOrders = orders.Where(o =>
            o.Status != OrderStatus.Cancelled &&
            o.Status != OrderStatus.Completed &&
            o.Senior?.CustomerId == customerId);

        foreach (var order in customerOrders)
        {
            var trackedOrder = await _orderRepository.LoadOrderWithIncludes(order.Id, new OrderIncludeOptions
            {
                Schedules = true,
                SchedulesJobRequests = true,
                ScheduleAssignments = true,
                AssignmentsJobInstances = true,
            }, asNoTracking: false);

            if (trackedOrder != null)
            {
                await _orderCancellationHandler.CancelOrderAsync(trackedOrder);
                await _orderRepository.UpdateAsync(trackedOrder);
                _logger.LogInformation("Cancelled order {OrderId} for suspended customer {CustomerId}", order.Id, customerId);
            }
        }
    }

    public async Task<SuspensionLogDto> ActivateUserAsync(int userId, int adminId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (!user.IsSuspended)
            throw new InvalidOperationException("User is not suspended.");

        user.IsSuspended = false;
        user.SuspensionReason = null;
        user.SuspendedAt = null;
        user.SuspendedByAdminId = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var log = new SuspensionLog
        {
            UserId = userId,
            Action = SuspensionAction.Activated,
            AdminId = adminId
        };

        await _suspensionLogRepository.AddAsync(log);

        return _mapper.Map<SuspensionLogDto>(log);
    }
}
