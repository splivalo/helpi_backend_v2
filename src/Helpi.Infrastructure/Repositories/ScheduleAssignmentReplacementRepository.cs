namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ScheduleAssignmentReplacementRepository : IScheduleAssignmentReplacementRepository
{
        private readonly AppDbContext _context;

        public ScheduleAssignmentReplacementRepository(AppDbContext context) => _context = context;

        public async Task<ScheduleAssignmentReplacement> GetByIdAsync(int id)
            => await _context.ScheduleAssignmentReplacements
                .Include(sar => sar.OriginalAssignment)
                .Include(sar => sar.NewAssignment)
                .FirstOrDefaultAsync(sar => sar.Id == id);

        public async Task<IEnumerable<ScheduleAssignmentReplacement>> GetByOriginalAssignmentAsync(int originalId)
            => await _context.ScheduleAssignmentReplacements
                .Where(sar => sar.OriginalAssignmentId == originalId)
                .ToListAsync();

        public async Task<IEnumerable<ScheduleAssignmentReplacement>> GetByNewAssignmentAsync(int newId)
            => await _context.ScheduleAssignmentReplacements
                .Where(sar => sar.NewAssignmentId == newId)
                .ToListAsync();

        public async Task<ScheduleAssignmentReplacement> AddAsync(ScheduleAssignmentReplacement replacement)
        {
                await _context.ScheduleAssignmentReplacements.AddAsync(replacement);
                await _context.SaveChangesAsync();
                return replacement;
        }

        public async Task UpdateAsync(ScheduleAssignmentReplacement replacement)
        {
                _context.ScheduleAssignmentReplacements.Update(replacement);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ScheduleAssignmentReplacement replacement)
        {
                _context.ScheduleAssignmentReplacements.Remove(replacement);
                await _context.SaveChangesAsync();
        }
}
