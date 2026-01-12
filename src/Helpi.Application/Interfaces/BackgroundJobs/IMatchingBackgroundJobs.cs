
namespace Helpi.Application.Interfaces.BackgroundJobs;

public interface IMatchingBackgroundJobs
{
    string? ScheduleFindAndNotifyStudents(int orderId, string? hangFireMatchingJobId, DateTime executionTime);
    void ScheduleJobInstanceMatching(int jobInstanceId, int reassignmentRecordId, DateTime executionTime);

}