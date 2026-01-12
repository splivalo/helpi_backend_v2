using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IScheduleAssignmentRepository
{
    Task<ScheduleAssignment?> GetByIdAsync(int id);

    Task<ScheduleAssignment?> LoadAssignmentWithIncludes(int assignmentId, AssignmentIncludeOptions options);
    Task<IEnumerable<ScheduleAssignment>> GetByStudentAsync(int studentId);
    Task<List<ScheduleAssignment>> GetAssignmentsNeedingJobGenerationAsync();
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

    void Detach(ScheduleAssignment assignment);

}
