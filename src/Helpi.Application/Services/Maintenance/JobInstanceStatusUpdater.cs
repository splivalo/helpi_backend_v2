using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services.Maintenance;

public class JobInstanceStatusUpdater
{
    private readonly ILogger<JobInstanceStatusUpdater> _logger;

    public JobInstanceStatusUpdater(ILogger<JobInstanceStatusUpdater> logger)
    {
        _logger = logger;
    }

    public void Update(Order order)
    {
        foreach (var schedule in order.Schedules)
        {
            foreach (var assignment in schedule.Assignments)
            {
                var jobInstances = assignment.JobInstances.Where(j => j.NeedsSubstitute == false);

                foreach (var job in jobInstances)
                {

                    var noneTerminalState = new[]{
                        JobInstanceStatus.Upcoming,
                        JobInstanceStatus.InProgress
                    };


                    if (schedule.IsCancelled && noneTerminalState.Contains(job.Status))
                    {
                        job.Status = JobInstanceStatus.Cancelled;
                        continue;
                    }

                    if (assignment.Status == AssignmentStatus.Terminated && noneTerminalState.Contains(job.Status))
                    {
                        job.Status = JobInstanceStatus.Cancelled;
                        continue;
                    }

                    var now = DateOnly.FromDateTime(DateTime.UtcNow);
                    var hasPassed = now > job.ScheduledDate;



                    if (hasPassed && noneTerminalState.Contains(job.Status))
                    {
                        job.Status = JobInstanceStatus.Cancelled;
                        continue;
                    }

                }
            }
        }
    }
}
