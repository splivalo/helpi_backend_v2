
using System.Linq.Expressions;
using Hangfire;
using Helpi.Application.Interfaces.BackgroundJobs;

namespace Helpi.Infrastructure.Services;

public class HangfireService : IHangfireService
{
    private readonly IBackgroundJobClient _backgroundJob;

    public HangfireService(IBackgroundJobClient backgroundJob)
    {
        _backgroundJob = backgroundJob;
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset delay)
    {
        return _backgroundJob.Schedule(methodCall, delay);
    }


    public void CancelScheduledJob(string jobId)
    {
        _backgroundJob.Delete(jobId);
    }
}