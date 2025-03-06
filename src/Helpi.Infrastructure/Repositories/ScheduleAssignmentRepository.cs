namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
public class ScheduleAssignmentRepository : IScheduleAssignmentRepository
{
        private readonly AppDbContext _context;

        public ScheduleAssignmentRepository(AppDbContext context) => _context = context;

        public async Task<ScheduleAssignment> GetByIdAsync(int id)
            => await _context.ScheduleAssignments
                .Include(sa => sa.OrderSchedule)
                .Include(sa => sa.Student)
                .FirstOrDefaultAsync(sa => sa.Id == id);

        public async Task<IEnumerable<ScheduleAssignment>> GetByStudentAsync(int studentId)
            => await _context.ScheduleAssignments
                .Where(sa => sa.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<ScheduleAssignment>> GetActiveAssignmentsAsync()
            => await _context.ScheduleAssignments
                .Where(sa => sa.Status == AssignmentStatus.Accepted ||
                    sa.Status == AssignmentStatus.Pending)
                .ToListAsync();

        public async Task<ScheduleAssignment> AddAsync(ScheduleAssignment assignment)
        {
                await _context.ScheduleAssignments.AddAsync(assignment);
                await _context.SaveChangesAsync();
                return assignment;
        }

        public async Task UpdateAsync(ScheduleAssignment assignment)
        {
                _context.ScheduleAssignments.Update(assignment);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ScheduleAssignment assignment)
        {
                _context.ScheduleAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
        }
}