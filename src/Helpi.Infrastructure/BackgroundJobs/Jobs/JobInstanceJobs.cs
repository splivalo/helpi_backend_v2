
using Hangfire;
using Helpi.Application.Interfaces.Services;

namespace Helpi.Infrastructure.BackgroundJobs.Jobs;

public class JobInstanceJobs : IJobInstanceJobs
{


    public void GenerateFutureJobInstances()
    {
        RecurringJob.AddOrUpdate<HangfireRecurringJobService>(
     "generate-job-instances",
     s => s.GenerateFutureJobInstances(),
     Cron.Daily); // Runs daily to check for new instances

    }

    public void ScheduleDailyStatusUpdates()
    {
        RecurringJob.AddOrUpdate<HangfireRecurringJobService>(
        "schedule-daily-job-instance-status",
        s => s.ScheduleDailyJobInstanceStatusUpdates(),
        Cron.Daily);
    }
    public void ScheduleDailyJobInstancePayments()
    {
        RecurringJob.AddOrUpdate<HangfireRecurringJobService>(
        "schedule-daily-job-intsance-payments",
        s => s.ScheduleDailyJobInstancePayments(),
        Cron.Daily);
    }

}