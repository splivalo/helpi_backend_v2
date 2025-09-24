using Hangfire;
using Helpi.Application.Services;

namespace Helpi.Infrastructure.BackgroundJobs.Jobs;

public class StudentBackgroundJobs
{

    public void ProcessStudentContracts()
    {
        RecurringJob.AddOrUpdate<StudentStatusService>(
     "process-student-contracts",
     s => s.ProcessStudentContracts(),
     Cron.Daily);

    }
}