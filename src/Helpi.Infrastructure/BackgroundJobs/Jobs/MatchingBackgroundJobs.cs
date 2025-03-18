
using Hangfire;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Infrastructure.BackgroundJobs.Jobs;

public class MatchingBackgroundJobs : IMatchingBackgroundJobs
{





    public void ScheduleFindAndNotifyStudents(int orderId, DateTime executionTime)
    {
        BackgroundJob.Schedule<IMatchingService>(
           service => service.FindAndNotifyStudentsAsync(orderId),
           executionTime
       );

    }


}