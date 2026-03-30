namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
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
                 .AsNoTracking()
                 .ToListAsync();
        }

        public async Task<IEnumerable<StudentAvailabilitySlot>> GetByDayAndTimeRangeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end)
        {
                return await _context.StudentAvailabilitySlots
                        .Where(s => s.StudentId == studentId &&
                        s.DayOfWeek == (byte)day &&
                        s.StartTime <= end &&
                        s.EndTime >= start)
                         .AsNoTracking()
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

        public async Task<List<StudentAvailabilitySlot>> AddRangeAsync(List<StudentAvailabilitySlot> slots)
        {
                await _context.StudentAvailabilitySlots.AddRangeAsync(slots);
                await _context.SaveChangesAsync();
                return slots;
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

        public async Task<List<StudentAvailabilitySlot>> UpdateRangeAsync(List<StudentAvailabilitySlot> slots)
        {
                _context.StudentAvailabilitySlots.UpdateRange(slots);
                await _context.SaveChangesAsync();
                return slots;
        }
        public async Task DeleteRangeAsync(List<StudentAvailabilitySlot> slots)
        {
                _context.StudentAvailabilitySlots.RemoveRange(slots);
                await _context.SaveChangesAsync();

        }

        public async Task RemoveAllByStudentIdAsync(int studentId)
        {
                var availabilitySlots = await _context.StudentAvailabilitySlots
                    .Where(slot => slot.StudentId == studentId)
                    .ToListAsync();

                _context.StudentAvailabilitySlots.RemoveRange(availabilitySlots);
                await _context.SaveChangesAsync();
        }



}