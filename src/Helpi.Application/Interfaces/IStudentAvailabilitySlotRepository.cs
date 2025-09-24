using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IStudentAvailabilitySlotRepository
{
    Task<StudentAvailabilitySlot?> GetByIdAsync(int studentId, byte dayOfWeek);
    Task<IEnumerable<StudentAvailabilitySlot>> GetByStudentAsync(int studentId);

    Task<IEnumerable<StudentAvailabilitySlot>> GetByDayAndTimeRangeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end);
    Task<StudentAvailabilitySlot> AddAsync(StudentAvailabilitySlot slot);
    Task<List<StudentAvailabilitySlot>> AddRangeAsync(List<StudentAvailabilitySlot> slots);
    Task UpdateAsync(StudentAvailabilitySlot slot);
    Task DeleteAsync(StudentAvailabilitySlot slot);
    Task<List<StudentAvailabilitySlot>> UpdateRangeAsync(List<StudentAvailabilitySlot> slots);
    Task DeleteRangeAsync(List<StudentAvailabilitySlot> slots);
    Task RemoveAllByStudentIdAsync(int userId);
}