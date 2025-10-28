using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface IJobInstanceRepository
{
    Task<JobInstance?> GetByIdSlimAsync(int id);
    Task<JobInstance> GetByIdAsync(int id);
    Task<IEnumerable<JobInstance>> GetByAssignmentAsync(int assignmentId);
    Task<IEnumerable<JobInstance>> GetJobInstancesByStudentAsync(int studentId);

    Task<IEnumerable<JobInstance>> GetJobInstances();
    Task<IEnumerable<JobInstance>> GetUpcomingJobsAsync(DateTime cutoff);
    Task<JobInstance> AddAsync(JobInstance instance);
    Task AddRangeAsync(List<JobInstance> jobInstances);
    Task UpdateAsync(JobInstance instance);
    Task UpdateRangeAsync(List<JobInstance> jobInstances);
    Task DeleteAsync(JobInstance instance);
    Task<IEnumerable<JobInstance>> GetSeniorCompletedJobInstances(int seniorId);
    Task<IEnumerable<JobInstance>> GetStudentCompletedJobInstances(int studentId);
    Task<IEnumerable<JobInstance>> GetStudentUpComingJobInstances(int studentId);

    Task<JobInstance?> UpdateToInProgressAsync(int jobInstanceId);
    // Task<JobInstance?> UpdateToCompletedAsync(int jobInstanceId);
    Task<JobInstance?> LoadJobInstanceWithIncludes(int jobInstanceId, JobInstanceIncludeOptions includes);
    Task<List<JobInstance>> GetByDateAsync(DateOnly today);
    Task SaveChangesAsync();
    Task<int> SumAsync(Expression<Func<JobInstance, bool>> predicate, Expression<Func<JobInstance, int>> selector);
    Task<List<JobInstance>> GetJobInstancesAsync(int? assignmentId,
int? prevAssignmentId,
     JobInstanceStatus? status,
     JobInstanceIncludeOptions options);
    void Detach(JobInstance jobInstance);
    Task<List<JobInstance>> GetFromDateForScheduleAsync(DateOnly fromDate, int scheduleId);
    void MarkForDeleteRange(IEnumerable<JobInstance> jobs);

    Task<IEnumerable<JobInstance>> GetCompletedJobInstancesForStudentAsync(int studentId, DateTime fromDate, DateTime toDate);
    Task<decimal> GetTotalCompletedHoursForPeriodAsync(int studentId, DateTime startDate, DateTime endDate);

}