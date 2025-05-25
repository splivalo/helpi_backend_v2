using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class CompletionStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderScheduleRepository _scheduleRepository;
    private readonly IScheduleAssignmentRepository _scheduldeAssignmentRepo;
    private readonly ILogger<CompletionStatusService> _logger;

    public CompletionStatusService(
        IOrderRepository orderRepository,
        IOrderScheduleRepository scheduleRepository,
        IScheduleAssignmentRepository scheduldeAssignmentRepo,
        ILogger<CompletionStatusService> logger
    )
    {
        _orderRepository = orderRepository;
        _scheduleRepository = scheduleRepository;
        _scheduldeAssignmentRepo = scheduldeAssignmentRepo;
        _logger = logger;
    }

    public async Task ProcessCompletionStatuses(int orderId)
    {
        _logger.LogInformation("🔍 Starting ProcessCompletionStatuses for Order ID: {OrderId}", orderId);

        var order = await _orderRepository.LoadOrderWithIncludes(orderId, new OrderIncludeOptions
        {
            Schedules = true,
            ScheduleAssignments = true,
            AssignmentsJobInstances = true
        });

        if (order == null)
        {
            _logger.LogWarning("⚠️ Order ID {OrderId} not found. Skipping...", orderId);
            return;
        }

        var orderSchedules = order.Schedules;

        foreach (var schedule in orderSchedules)
        {
            _logger.LogInformation("📅 Processing Schedule ID: {ScheduleId}", schedule.Id);

            var scheduleAssignments = schedule.Assignments;

            foreach (var assignment in scheduleAssignments)
            {
                _logger.LogInformation("👷 Processing Assignment ID: {AssignmentId}", assignment.Id);
                await ProcessAssignmentCompletion(assignment);
            }
        }

        await ProcessOrderCompletion(order.Id);
        _logger.LogInformation("✅ Finished processing completion statuses for Order ID: {OrderId}", orderId);
    }

    public async Task<bool> ProcessAssignmentCompletion(ScheduleAssignment assignment)
    {
        _logger.LogInformation("🧩 Checking assignment completion for ID: {AssignmentId}", assignment.Id);

        var allJobsComplete = assignment.JobInstances.All(ji =>
            ji.Status is JobInstanceStatus.Completed or JobInstanceStatus.Cancelled);

        if (!allJobsComplete)
        {
            _logger.LogInformation("🕒 Assignment ID {AssignmentId} has incomplete jobs. Skipping...", assignment.Id);
            return false;
        }

        assignment.Status = AssignmentStatus.Completed;
        assignment.CompletedAt = DateTime.UtcNow;
        await _scheduldeAssignmentRepo.UpdateAsync(assignment);

        _logger.LogInformation("🏁 Assignment ID {AssignmentId} marked as completed.", assignment.Id);
        return true;
    }

    public async Task<bool> ProcessOrderCompletion(int orderId)
    {
        _logger.LogInformation("📦 Checking order completion for Order ID: {OrderId}", orderId);

        var allAssignmentsCompleted = await _scheduldeAssignmentRepo.IsAllOrderAssignmentsCompleted(orderId);

        if (!allAssignmentsCompleted)
        {
            _logger.LogInformation("🔄 Order ID {OrderId} has incomplete assignments.", orderId);
            return false;
        }

        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            _logger.LogWarning("❌ Order ID {OrderId} not found during completion update.", orderId);
            return false;
        }

        order.Status = OrderStatus.Completed;
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("🎉 Order ID {OrderId} marked as completed!", orderId);
        return true;
    }
}

