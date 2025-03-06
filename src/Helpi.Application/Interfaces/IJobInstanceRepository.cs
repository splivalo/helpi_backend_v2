using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IJobInstanceRepository
{
    Task<JobInstance> GetByIdAsync(int id);
    Task<IEnumerable<JobInstance>> GetByAssignmentAsync(int assignmentId);
    Task<IEnumerable<JobInstance>> GetUpcomingJobsAsync(DateTime cutoff);
    Task<JobInstance> AddAsync(JobInstance instance);
    Task UpdateAsync(JobInstance instance);
    Task DeleteAsync(JobInstance instance);
}
