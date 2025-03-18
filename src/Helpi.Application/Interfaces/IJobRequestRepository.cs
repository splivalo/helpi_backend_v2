using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IJobRequestRepository
{
    Task<JobRequest?> GetByIdAsync(int id);
    Task<IEnumerable<JobRequest>> GetPendingRequestsAsync();
    Task<IEnumerable<JobRequest>> GetExpiredRequestsAsync();
    Task<JobRequest> AddAsync(JobRequest request);
    Task UpdateAsync(JobRequest request);
    Task DeleteAsync(JobRequest request);
    Task<List<int>> NotifiedStudentIds(int orderScheduleId);

}