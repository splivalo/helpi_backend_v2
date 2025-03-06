namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StudentAvailabilitySlotRepository : IStudentAvailabilitySlotRepository
{
        private readonly AppDbContext _context;

        public StudentAvailabilitySlotRepository(AppDbContext context) => _context = context;

        public async Task<StudentAvailabilitySlot> GetByIdAsync(int id)
            => await _context.StudentAvailabilitySlots.FindAsync(id);

        public async Task<IEnumerable<StudentAvailabilitySlot>> GetByStudentAsync(int studentId)
            => await _context.StudentAvailabilitySlots
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<StudentAvailabilitySlot>> GetByDayAndTimeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end)
            => await _context.StudentAvailabilitySlots
                .Where(s => s.StudentId == studentId &&
                    s.DayOfWeek == (byte)day &&
                    s.StartTime <= end &&
                    s.EndTime >= start)
                .ToListAsync();

        public async Task<StudentAvailabilitySlot> AddAsync(StudentAvailabilitySlot slot)
        {
                await _context.StudentAvailabilitySlots.AddAsync(slot);
                await _context.SaveChangesAsync();
                return slot;
        }

        public async Task UpdateAsync(StudentAvailabilitySlot slot)
        {
                _context.StudentAvailabilitySlots.Update(slot);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(StudentAvailabilitySlot slot)
        {
                _context.StudentAvailabilitySlots.Remove(slot);
                await _context.SaveChangesAsync();
        }
}