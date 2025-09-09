using Helpi.Application.DTOs;
using Helpi.Domain.Entities;


namespace Helpi.Application.Interfaces.Services;

public interface IJobInstanceService
{

    Task<List<JobInstanceDto>> GetJobInstancesByAssignmentAsync(int assignmentId);
    Task<List<JobInstanceDto>> GetJobInstancesByStudentAsync(int studentId);
    Task<List<JobInstanceDto>> GetJobInstances();
    Task<List<JobInstanceDto>> GetSeniorCompletedJobInstances(int seniorId);
    Task<List<JobInstanceDto>> GetStudentCompletedJobInstances(int studentId);
    Task<List<JobInstanceDto>> GetStudentUpComingJobInstances(int studentId);

    Task UpdateToInProgressAsync(int jobInstanceId);
    Task UpdateToCompletedAsync(int jobInstanceId);
    Task RequestJobReviewAsync(int jobInstanceId);

    Task<JobInstance?> ManageJobInstance(
            int jobInstanceId,
            DateOnly? newDate,
            TimeOnly? newStartTime,
            TimeOnly? newEndTime,
            string reason,
            int? preferedStudentId,
            bool reassignStudent,
            int requestedByUserId);
}