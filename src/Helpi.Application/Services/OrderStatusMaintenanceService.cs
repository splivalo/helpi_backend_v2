using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Services.Maintenance;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class OrderStatusMaintenanceService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderStatusMaintenanceService> _logger;
    private readonly OrderCancellationHandler _orderCancellationHandler;
    private readonly JobInstanceStatusUpdater _jobUpdater;
    private readonly AssignmentStatusUpdater _assignmentUpdater;
    private readonly ScheduleStatusUpdater _scheduleUpdater;
    private readonly OrderStatusUpdater _orderUpdater;

    public OrderStatusMaintenanceService(
        IOrderRepository orderRepository,
        ILogger<OrderStatusMaintenanceService> logger,
        OrderCancellationHandler orderCancellationHandler,
        JobInstanceStatusUpdater jobUpdater,
        AssignmentStatusUpdater assignmentUpdater,
        ScheduleStatusUpdater scheduleUpdater,
        OrderStatusUpdater orderUpdater
    )
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _orderCancellationHandler = orderCancellationHandler;
        _jobUpdater = jobUpdater;
        _assignmentUpdater = assignmentUpdater;
        _scheduleUpdater = scheduleUpdater;
        _orderUpdater = orderUpdater;
    }

    public async Task MaintainOrderStatuses(int orderId, bool skipJobInstanceUpdate = false)
    {
        try
        {
            _logger.LogInformation("🔍 Maintaining statuses for Order {OrderId}", orderId);
            // _orderRepository.DetachAllEntities();

            var order = await _orderRepository.LoadOrderWithIncludes(orderId, new OrderIncludeOptions
            {
                Schedules = true,
                SchedulesJobRequests = true,
                ScheduleAssignments = true,
                AssignmentsJobInstances = true,
            },
            asNoTracking: false);

            if (order == null)
            {
                _logger.LogWarning("⚠️ Order {OrderId} not found", orderId);
                return;
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                await _orderCancellationHandler.CancelOrderAsync(order);
                await _orderRepository.UpdateAsync(order);
                return;
            }

            // Update sequence matters (outer -> inner -> aggregate):
            // 1. ScheduleStatusUpdater   : Schedules may be cancelled, which cascades down.
            // 2. AssignmentStatusUpdater : Assignment state depends on schedule state and job instances.
            // 3. JobInstanceStatusUpdater: Job instance state depends on assignment & schedule states.
            // 4. OrderStatusUpdater      : Order state is derived from all schedules/assignments/jobs.
            _scheduleUpdater.Update(order);
            _assignmentUpdater.Update(order);
            if (!skipJobInstanceUpdate)
            {
                _jobUpdater.Update(order);
            }
            _orderUpdater.Update(order);

            // _orderRepository.DetachAllEntities();

            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation("✅ Order {OrderId} maintenance complete", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ERROR]  Order {OrderId} maintenance ", orderId);
            throw;
        }
    }
}


