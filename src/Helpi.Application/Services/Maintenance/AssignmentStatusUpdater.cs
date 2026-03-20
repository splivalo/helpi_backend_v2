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

                // If there are active (non-terminal) jobs, ensure assignment is Accepted
                var hasActiveJobs = assignment.JobInstances.Any(ji => !ji.IsTerminal);

                if (hasActiveJobs && assignment.Status == AssignmentStatus.Completed)
                {
                    assignment.Status = AssignmentStatus.Accepted;
                    continue;
                }

                if (assignment.IsTerminal) continue;

                var allJobsTerminal = assignment.JobInstances.All(ji => ji.IsTerminal);

                // Only mark Completed if ALL jobs are Completed (not just terminal)
                var allJobsCompleted = assignment.JobInstances.All(ji =>
                    ji.Status == JobInstanceStatus.Completed ||
                    ji.Status == JobInstanceStatus.Rescheduled);

                if (allJobsCompleted && assignment.JobInstances.Any())
                {
                    assignment.Status = AssignmentStatus.Completed;
                }
                else if (allJobsTerminal)
                {
                    // All jobs are terminal but not all completed (some cancelled)
                    // Keep assignment as-is (Accepted) — don't mark Completed
                }
            }
        }

    }
}
