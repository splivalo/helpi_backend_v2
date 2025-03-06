using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IScheduleAssignmentReplacementRepository
{
    Task<ScheduleAssignmentReplacement> GetByIdAsync(int id);
    Task<IEnumerable<ScheduleAssignmentReplacement>> GetByOriginalAssignmentAsync(int originalId);
    Task<IEnumerable<ScheduleAssignmentReplacement>> GetByNewAssignmentAsync(int newId);
    Task<ScheduleAssignmentReplacement> AddAsync(ScheduleAssignmentReplacement replacement);
    Task UpdateAsync(ScheduleAssignmentReplacement replacement);
    Task DeleteAsync(ScheduleAssignmentReplacement replacement);
}