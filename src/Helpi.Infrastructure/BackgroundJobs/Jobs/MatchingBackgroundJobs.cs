
using Hangfire;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Infrastructure.BackgroundJobs.Jobs;

public class MatchingBackgroundJobs : IMatchingBackgroundJobs
{

    public void ScheduleFindAndNotifyStudents(int orderId, DateTime executionTime)
    {
        BackgroundJob.Schedule<IMatchingService>(
           service => service.InitiateMatchingProcessAsync(orderId),
           executionTime
       );

    }
    public void ScheduleJobInstanceMatching(int jobInstanceId, int reassignmentRecordId, DateTime executionTime)
    {
        BackgroundJob.Schedule<IJobInstanceMatchingService>(
           service => service.ProcessJobInstanceMatchingAsync(
                        jobInstanceId,
                        reassignmentRecordId
                        ),
           executionTime
       );

    }


}