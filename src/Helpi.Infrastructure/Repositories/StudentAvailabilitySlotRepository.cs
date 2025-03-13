namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StudentAvailabilitySlotRepository : IStudentAvailabilitySlotRepository
{
        private readonly AppDbContext _context;

        public StudentAvailabilitySlotRepository(AppDbContext context) => _context = context;

        public async Task<StudentAvailabilitySlot?> GetByIdAsync(int studentId, byte dayOfWeek)
        {
                return await _context.StudentAvailabilitySlots
                        .SingleOrDefaultAsync(s => s.StudentId == studentId
                         && s.DayOfWeek == dayOfWeek);

        }

        public async Task<IEnumerable<StudentAvailabilitySlot>> GetByStudentAsync(int studentId)
        {
                return await _context.StudentAvailabilitySlots
                 .Where(s => s.StudentId == studentId)
                 .ToListAsync();
        }

        public async Task<IEnumerable<StudentAvailabilitySlot>> GetByDayAndTimeRangeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end)
        {
                return await _context.StudentAvailabilitySlots
                        .Where(s => s.StudentId == studentId &&
                        s.DayOfWeek == (byte)day &&
                        s.StartTime <= end &&
                        s.EndTime >= start)
                        .ToListAsync();
        }

        public async Task<StudentAvailabilitySlot> AddAsync(StudentAvailabilitySlot slot)
        {
                // _context.Attach(new Student { UserId = slot.StudentId });
                // _context.Entry(slot).Property(x => x.StudentId).IsModified = false;

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