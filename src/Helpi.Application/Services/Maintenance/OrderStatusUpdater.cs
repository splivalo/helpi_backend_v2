using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services.Maintenance;

public class OrderStatusUpdater
{
    private readonly ILogger<OrderStatusUpdater> _logger;

    public OrderStatusUpdater(ILogger<OrderStatusUpdater> logger)
    {
        _logger = logger;
    }

    public void Update(Order order)
    {
        if (order.Status == OrderStatus.Cancelled)
        {
            return;
        }

        // Collect all job instances from all assignments
        var allJobInstances = order.Schedules
            .SelectMany(s => s.Assignments)
            .SelectMany(a => a.JobInstances)
            .ToList();

        // If there were job instances but none remain scheduled (all completed/cancelled),
        // determine final order status based on what happened to sessions.
        if (allJobInstances.Any() &&
            !allJobInstances.Any(j => j.Status == JobInstanceStatus.Upcoming))
        {
            var hasCompletedSessions = allJobInstances.Any(j => j.Status == JobInstanceStatus.Completed);

            if (hasCompletedSessions)
            {
                // At least one session was completed → order is Completed
                if (order.Status != OrderStatus.Completed)
                {
                    order.Status = OrderStatus.Completed;
                    _logger.LogInformation("🎉 Order {OrderId} marked Completed (has completed sessions)", order.Id);
                }
                return;
            }

            // ALL sessions were cancelled, none completed
            // For recurring orders, don't auto-cancel — new sessions may be generated
            // when student extends contract. Senior must use "Cancel Order" explicitly.
            if (order.IsRecurring)
            {
                _logger.LogInformation("📋 Order {OrderId} is recurring — staying Active despite all sessions cancelled", order.Id);
                // Don't change status, fall through to assignment-based logic
            }
            else
            {
                // One-time order with all sessions cancelled → Cancelled
                if (order.Status != OrderStatus.Cancelled)
                {
                    order.Status = OrderStatus.Cancelled;
                    _logger.LogInformation("❌ Order {OrderId} (one-time) marked Cancelled (all sessions cancelled)", order.Id);
                }
                return;
            }
        }

        var allAssignments = order.Schedules.SelectMany(s => s.Assignments).ToList();
        if (allAssignments.Any() && allAssignments.All(a => a.Status == AssignmentStatus.Completed))
        {
            if (order.Status != OrderStatus.Completed)
            {
                order.Status = OrderStatus.Completed;
                _logger.LogInformation("🎉 Order {OrderId} marked Completed", order.Id);
            }
            return;
        }

        var activeSchedules = order.Schedules.Where(s => !s.IsCancelled).ToList();
        var assignedStatus = new[] { AssignmentStatus.Accepted, AssignmentStatus.Completed };
        var fullyAssigned = activeSchedules.All(s => s.Assignments.Any(a => assignedStatus.Contains(a.Status)));

        var newStatus = fullyAssigned ? OrderStatus.FullAssigned : OrderStatus.Pending;

        if (order.Status != newStatus)
        {
            _logger.LogInformation("📋 Order {OrderId} status updated {Old} → {New}", order.Id, order.Status, newStatus);
            order.Status = newStatus;
        }
    }
}
