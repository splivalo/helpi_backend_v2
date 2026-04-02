
namespace Helpi.Application.Interfaces.Services;

public interface IJobInstanceJobs
{
    public void GenerateFutureJobInstances();
    public void ScheduleDailyStatusUpdates();
    public void ScheduleDailyJobInstancePayments();
    public void RetryFailedInvoices();
}