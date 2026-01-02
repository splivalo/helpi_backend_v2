
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Infrastructure.BackgroundJobs.Jobs;

public class MatchingBackgroundJobs : IMatchingBackgroundJobs
{


    public string? ScheduleFindAndNotifyStudents(
     int orderId,
     string? hangFireMatchingJobId,
     DateTime executionTime)
    {
        if (!string.IsNullOrEmpty(hangFireMatchingJobId))
        {
            using var connection = JobStorage.Current.GetConnection();
            var state = connection.GetStateData(hangFireMatchingJobId);

            // If a job with this ID is already scheduled, skip scheduling
            if (state?.Name == ScheduledState.StateName ||
            state?.Name == EnqueuedState.StateName)
            {
                return null;
            }
        }

        // Otherwise, schedule a new job
        return BackgroundJob.Schedule<JobRunner>(
            service => service.InitiateMatchingProcessAsync(orderId),
            executionTime
        );
    }

    public void ScheduleJobInstanceMatching(int jobInstanceId, int reassignmentRecordId, DateTime executionTime)
    {

        BackgroundJob.Schedule<JobRunner>(
           service => service.ProcessJobInstanceMatchingAsync(
                        jobInstanceId,
                        reassignmentRecordId
                        ),
           executionTime
       );

    }


}