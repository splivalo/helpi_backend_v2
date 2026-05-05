using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IScheduleAssignmentRepository
{
    Task<ScheduleAssignment?> GetByIdAsync(int id);

    Task<ScheduleAssignment?> LoadAssignmentWithIncludes(int assignmentId, AssignmentIncludeOptions options);
    Task<IEnumerable<ScheduleAssignment>> GetByStudentAsync(int studentId);
    Task<IEnumerable<ScheduleAssignment>> GetByStudentWithDetailsAsync(int studentId);
    Task<List<ScheduleAssignment>> GetAssignmentsNeedingJobGenerationAsync();
    Task<List<ScheduleAssignment>> GetAssignmentsNeedingJobGenerationForStudentAsync(int studentId);
    Task<ScheduleAssignment> AddAsync(ScheduleAssignment assignment);
    Task UpdateAsync(ScheduleAssignment assignment);
    Task DeleteAsync(ScheduleAssignment assignment);
    Task<Student?> GetActiveStudentForOrderScheduleAsync(int orderScheduleId);
    Task<ScheduleAssignment?> GetAssignmentForOrderScheduleAsync(int orderScheduleId);
    Task<bool> IsScheduleAssigned(int scheduleId);
    Task<bool> IsAllOrderAssignmentsCompleted(int orderId);
    Task<bool> IsScheduleCompleted(int scheduleId);
    Task<List<ScheduleAssignment>> GetActiveAssignmentsByStudentId(int studentId);

    Task<bool> HasActiveAssignmentsForServicesAsync(int studentId, List<int> serviceIds);

    Task<bool> HasActiveAssignmentsForSlotsAsync(int studentId, List<StudentAvailabilitySlotCreateDto> dtos);

    Task<List<ScheduleAssignment>> GetConflictingAssignmentsAsync(int studentId, List<byte> removedDays);

    Task<List<ScheduleAssignment>> GetAllPendingAcceptanceAsync();

    /// <summary>
    /// Returns active (PendingAcceptance / Accepted) per-session sub-assignments that
    /// are linked to a specific JobInstance — either directly (the sub-assignment is
    /// currently attached to the JI) or transitively (an earlier JI version pointed
    /// at it via PrevAssignmentId). Used during cancel/restore cycles to terminate
    /// stale leftovers and avoid duplicate sessions on student-accept.
    /// </summary>
    Task<List<ScheduleAssignment>> GetActiveSubAssignmentsForJobInstanceAsync(int jobInstanceId);

    void Detach(ScheduleAssignment assignment);

}
