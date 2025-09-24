using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services.Maintenance;

public class OrderCancellationHandler
{
    private readonly IOrderScheduleRepository _scheduleRepository;
    private readonly ScheduleCancellationHandler _scheduleCancellationHandler;
    private readonly ILogger<OrderCancellationHandler> _logger;

    public OrderCancellationHandler(
        IOrderScheduleRepository scheduleRepository,
        ScheduleCancellationHandler scheduleCancellationHandler,
        ILogger<OrderCancellationHandler> logger
    )
    {
        _scheduleRepository = scheduleRepository;
        _scheduleCancellationHandler = scheduleCancellationHandler;
        _logger = logger;
    }

    public async Task CancelOrderAsync(Order order)
    {
        _logger.LogInformation("❌ Cancelling Order {OrderId}", order.Id);

        foreach (var schedule in order.Schedules)
        {
            await _scheduleCancellationHandler.CancelScheduleAsync(schedule, "Order cancelled");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        _logger.LogInformation("📦 Order {OrderId} marked Cancelled", order.Id);
    }
}
