using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IJobInstanceRepository
{
    Task<JobInstance> GetByIdAsync(int id);
    Task<IEnumerable<JobInstance>> GetByAssignmentAsync(int assignmentId);
    Task<IEnumerable<JobInstance>> GetJobInstancesByStudentAsync(int studentId);

    Task<IEnumerable<JobInstance>> GetJobInstances();
    Task<IEnumerable<JobInstance>> GetUpcomingJobsAsync(DateTime cutoff);
    Task<JobInstance> AddAsync(JobInstance instance);
    Task AddRangeAsync(List<JobInstance> jobInstances);
    Task UpdateAsync(JobInstance instance);
    Task DeleteAsync(JobInstance instance);
    Task<IEnumerable<JobInstance>> GetSeniorCompletedJobInstances(int seniorId);
    Task<IEnumerable<JobInstance>> GetStudentCompletedJobInstances(int studentId);
    Task<IEnumerable<JobInstance>> GetStudentUpComingJobInstances(int studentId);

    Task<JobInstance?> UpdateToInProgressAsync(int jobInstanceId);
    Task<JobInstance?> UpdateToCompletedAsync(int jobInstanceId);
    Task<List<JobInstance>> GetByDateAsync(DateOnly today);
    Task SaveChangesAsync();

}
