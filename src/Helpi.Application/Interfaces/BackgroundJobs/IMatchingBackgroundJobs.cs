
namespace Helpi.Application.Interfaces.BackgroundJobs;

public interface IMatchingBackgroundJobs
{
    void ScheduleFindAndNotifyStudents(int orderId, DateTime executionTime);
    void ScheduleJobInstanceMatching(int jobInstanceId, int reassignmentRecordId, DateTime executionTime);

}