using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;
public interface IStudentAvailabilitySlotRepository
{
    Task<StudentAvailabilitySlot> GetByIdAsync(int id);
    Task<IEnumerable<StudentAvailabilitySlot>> GetByStudentAsync(int studentId);
    Task<IEnumerable<StudentAvailabilitySlot>> GetByDayAndTimeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end);
    Task<StudentAvailabilitySlot> AddAsync(StudentAvailabilitySlot slot);
    Task UpdateAsync(StudentAvailabilitySlot slot);
    Task DeleteAsync(StudentAvailabilitySlot slot);
}