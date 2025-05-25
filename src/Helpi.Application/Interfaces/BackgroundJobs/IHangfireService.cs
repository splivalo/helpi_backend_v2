using System.Linq.Expressions;

namespace Helpi.Application.Interfaces.BackgroundJobs;

public interface IHangfireService
{
    string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);
    void CancelScheduledJob(string jobId);
}