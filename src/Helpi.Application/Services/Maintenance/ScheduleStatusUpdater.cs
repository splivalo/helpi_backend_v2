using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services.Maintenance;

public class ScheduleStatusUpdater
{
    private readonly ILogger<ScheduleStatusUpdater> _logger;

    public ScheduleStatusUpdater(ILogger<ScheduleStatusUpdater> logger)
    {
        _logger = logger;
    }

    public void Update(Order order)
    {
        foreach (var schedule in order.Schedules)
        {
            if (order.Status == OrderStatus.Cancelled)
            {
                if (schedule.IsCancelled) continue;

                schedule.IsCancelled = true;
            }
        }
    }
}
