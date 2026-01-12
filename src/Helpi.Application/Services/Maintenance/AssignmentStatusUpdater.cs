using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services.Maintenance;

public class AssignmentStatusUpdater
{
    private readonly ILogger<AssignmentStatusUpdater> _logger;

    public AssignmentStatusUpdater(ILogger<AssignmentStatusUpdater> logger)
    {
        _logger = logger;
    }

    public void Update(Order order)
    {
        foreach (var schedule in order.Schedules)
        {
            foreach (var assignment in schedule.Assignments)
            {

                if (schedule.IsCancelled)
                {
                    if (assignment.Status == AssignmentStatus.Accepted)
                    {
                        assignment.Status = AssignmentStatus.Terminated;
                    }
                    continue;
                }

                if (assignment.IsTerminal) continue;

                //
                // var jobInstances = assignment.JobInstances.Where(j => j.NeedsSubstitute == false);

                var allJobsTerminal = assignment.JobInstances.All(ji => ji.IsTerminal);

                if (allJobsTerminal)
                {
                    assignment.Status = AssignmentStatus.Completed;
                }

            }
        }

    }
}
