using Helpi.Application.DTOs;
using Helpi.Domain.Entities;


namespace Helpi.Application.Interfaces.Services;

public interface IJobInstanceService
{

    Task<List<SessionDto>> GetJobInstancesByAssignmentAsync(int assignmentId);
    Task<List<SessionDto>> GetJobInstancesByOrderAsync(int orderId);
    Task<List<SessionDto>> GetJobInstancesByOrderAsync(int orderId, DateOnly? from, DateOnly? to);
    Task<List<SessionDto>> GetJobInstancesByStudentAsync(int studentId);
    Task<List<SessionDto>> GetJobInstances();
    Task<List<SessionDto>> GetSeniorCompletedJobInstances(int seniorId);
    Task<List<SessionDto>> GetStudentCompletedJobInstances(int studentId);
    Task<List<SessionDto>> GetStudentUpComingJobInstances(int studentId);

    Task UpdateToInProgressAsync(int jobInstanceId);
    Task UpdateToCompletedAsync(int jobInstanceId);
    Task<bool> EnsureCompletedAsync(int jobInstanceId);
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
    Task<SessionDto?> CancelJobInstance(int jobInstanceId, bool isAdmin = false, string callerRole = "");
    Task<SessionDto?> ReactivateJobInstance(int jobInstanceId);

    /// <summary>
    /// Atomically reactivates a cancelled session and — optionally — updates
    /// its date/time in-place and/or assigns a new student (PendingAcceptance).
    /// Replaces the two-call (reactivate → manage) pattern that created orphaned
    /// "Rescheduled" sessions when both date and student changed together.
    /// </summary>
    Task<SessionDto?> ReactivateAndManageJobInstance(
        int jobInstanceId,
        DateOnly? newDate,
        TimeOnly? newStartTime,
        TimeOnly? newEndTime,
        int? preferredStudentId);

    /// <summary>
    /// Stamp CanCancel on each SessionDto based on caller role and PricingConfiguration cutoffs.
    /// </summary>
    Task StampCanCancelAsync(IEnumerable<SessionDto> sessions, string callerRole);

}