using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IJobRequestRepository
{
    Task<JobRequest?> GetByIdAsync(int id);
    Task<IEnumerable<JobRequest>> GetExpiredRequestsAsync();
    Task<JobRequest> AddAsync(JobRequest request);
    Task UpdateAsync(JobRequest request);
    Task DeleteAsync(JobRequest request);
    Task<List<int>> NotifiedStudentIds(int orderScheduleId);
    Task<List<JobRequest>> GetStudentPendingRequests(int studentId);
    Task<JobRequest> RespondToJobRequestAsync(JobRequest jobRequest);

}