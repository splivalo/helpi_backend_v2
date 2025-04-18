using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IScheduleAssignmentRepository
{
    Task<ScheduleAssignment> GetByIdAsync(int id);
    Task<IEnumerable<ScheduleAssignment>> GetByStudentAsync(int studentId);
    Task<List<ScheduleAssignment>> GetActiveAssignmentsAsync();
    Task<ScheduleAssignment> AddAsync(ScheduleAssignment assignment);
    Task UpdateAsync(ScheduleAssignment assignment);
    Task DeleteAsync(ScheduleAssignment assignment);
    Task<Student?> GetActiveStudentForOrderScheduleAsync(int orderScheduleId);

}
