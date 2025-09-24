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
