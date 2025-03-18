
namespace Helpi.Application.Interfaces.BackgroundJobs;
public interface IMatchingBackgroundJobs
{
    void ScheduleFindAndNotifyStudents(int orderId, DateTime executionTime);
}