using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IScheduleAssignmentRepository
{
    Task<ScheduleAssignment> GetByIdAsync(int id);

    Task<ScheduleAssignment?> LoadAssignmentWithIncludes(int assignmentId, AssignmentIncludeOptions options);
    Task<IEnumerable<ScheduleAssignment>> GetByStudentAsync(int studentId);
    Task<List<ScheduleAssignment>> GetActiveAssignmentsAsync();
    Task<ScheduleAssignment> AddAsync(ScheduleAssignment assignment);
    Task UpdateAsync(ScheduleAssignment assignment);
    Task DeleteAsync(ScheduleAssignment assignment);
    Task<Student?> GetActiveStudentForOrderScheduleAsync(int orderScheduleId);
    Task<bool> IsScheduleAssigned(int scheduleId);
    Task<bool> IsAllOrderAssignmentsCompleted(int orderId);
    Task<bool> IsScheduleCompleted(int scheduleId);
}
