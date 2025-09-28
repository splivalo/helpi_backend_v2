
using Hangfire;
using Hangfire.States;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Infrastructure.BackgroundJobs.Jobs;

public class MatchingBackgroundJobs : IMatchingBackgroundJobs
{

    public void ScheduleFindAndNotifyStudents(int orderId, DateTime executionTime)
    {

        var jobId = $"matching-order-{orderId}"; // prevents duplicates (will replace old schedule)

        using var connection = JobStorage.Current.GetConnection();
        var state = connection.GetStateData(jobId);

        // 👉 If a job with this ID is already scheduled, do nothing
        if (state?.Name == ScheduledState.StateName)
        {
            return;
        }

        BackgroundJob.Schedule<IMatchingService>(
            jobId,
           service => service.InitiateMatchingProcessAsync(orderId),
           executionTime
       );

    }
    public void ScheduleJobInstanceMatching(int jobInstanceId, int reassignmentRecordId, DateTime executionTime)
    {
        var jobId = $"matching-jobInstance-{jobInstanceId}";

        using var connection = JobStorage.Current.GetConnection();
        var state = connection.GetStateData(jobId);

        // 👉 If a job with this ID is already scheduled, do nothing
        if (state?.Name == ScheduledState.StateName)
        {
            return;
        }

        BackgroundJob.Schedule<IJobInstanceMatchingService>(
                        jobId,
           service => service.ProcessJobInstanceMatchingAsync(
                        jobInstanceId,
                        reassignmentRecordId
                        ),
           executionTime
       );

    }


}