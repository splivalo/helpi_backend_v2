using Helpi.Application.DTOs;
using Helpi.Domain.Entities;


namespace Helpi.Application.Interfaces.Services;

public interface IJobInstanceService
{

    Task<List<SessionDto>> GetJobInstancesByAssignmentAsync(int assignmentId);
    Task<List<SessionDto>> GetJobInstancesByOrderAsync(int orderId);
    Task<List<SessionDto>> GetJobInstancesByStudentAsync(int studentId);
    Task<List<SessionDto>> GetJobInstances();
    Task<List<SessionDto>> GetSeniorCompletedJobInstances(int seniorId);
    Task<List<SessionDto>> GetStudentCompletedJobInstances(int studentId);
    Task<List<SessionDto>> GetStudentUpComingJobInstances(int studentId);

    Task UpdateToInProgressAsync(int jobInstanceId);
    Task UpdateToCompletedAsync(int jobInstanceId);
    Task RemindStudentAsync(int jobInstanceId);
    Task RequestJobReviewAsync(int jobInstanceId);

    Task<SessionDto?> ManageJobInstance(
            int jobInstanceId,
            DateOnly? newDate,
            TimeOnly? newStartTime,
            TimeOnly? newEndTime,
            string reason,
            int? preferedStudentId,
            bool reassignStudent,
            int requestedByUserId);
    Task<SessionDto?> CancelJobInstance(int jobInstanceId);
    Task<SessionDto?> ReactivateJobInstance(int jobInstanceId);

}