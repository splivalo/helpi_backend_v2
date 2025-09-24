namespace Helpi.Application.Interfaces.Services;

public interface IJobInstanceMatchingService
{
    Task StartJobInstanceMatchingAsync(int jobInstanceId, int reassignmentRecordId);
    Task ProcessJobInstanceMatchingAsync(int jobInstanceId, int reassignmentRecordId);
}