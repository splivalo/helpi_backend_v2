namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.DTOs;
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

        public async Task<List<ScheduleAssignment>> GetActiveAssignmentsAsync()
        {
                return await _context.ScheduleAssignments
               .Include(sa => sa.OrderSchedule.Order)
               .Include(sa => sa.JobInstances)
               .Where(sa => sa.IsTemporary == false &&
                            sa.OrderSchedule.Order.IsRecurring &&
                            sa.OrderSchedule.Order.Status == OrderStatus.Acitve)
               .ToListAsync();
        }

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

        public async Task<Student?> GetActiveStudentForOrderScheduleAsync(int orderScheduleId)
        {
                return await _context.ScheduleAssignments
                    .Where(sa => sa.OrderScheduleId == orderScheduleId)
                    .Where(sa => sa.Status == AssignmentStatus.Accepted)
                    .OrderByDescending(sa => sa.AcceptedAt) // Get most recent
                    .Include(sa => sa.Student).ThenInclude(s => s.Contact)
                    .Select(sa => sa.Student)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
        }

        public async Task<bool> IsScheduleAssigned(int scheduleId)
        {
                var activeStatuses = new[]
                {
                        AssignmentStatus.Completed,
                        AssignmentStatus.Accepted
                };

                return await _context.ScheduleAssignments
                    .AnyAsync(sa => sa.OrderScheduleId == scheduleId && activeStatuses.Contains(sa.Status));
        }


        public async Task<bool> IsAllOrderAssignmentsCompleted(int orderId)
        {
                var hasActiveAssignments = await _context.ScheduleAssignments.AnyAsync(a =>
                        a.OrderId == orderId &&
                        a.Status != AssignmentStatus.Completed &&
                        a.Status != AssignmentStatus.Canceled
                );

                return hasActiveAssignments == false;

        }
        public async Task<bool> IsScheduleCompleted(int scheduleId)
        {
                var activeStatuses = new[]
                {
                        AssignmentStatus.Completed,
                        AssignmentStatus.Accepted
                };

                return await _context.ScheduleAssignments
                    .AnyAsync(sa => sa.OrderScheduleId == scheduleId && sa.Status == AssignmentStatus.Completed);
        }

        public async Task<ScheduleAssignment?> LoadAssignmentWithIncludes(int assignmentId, AssignmentIncludeOptions options)
        {
                var query = _context.ScheduleAssignments.AsQueryable();

                if (options.IncludeStudent)
                        query = query.Include(o => o.Student).ThenInclude(s => s.Contact);


                if (options.JobInstances)
                        query = query.Include(o => o.JobInstances);


                if (options.IncludeSchedules)
                {
                        if (options.IncludeScheduleAssignments)
                        {
                                query = query
                                    .Include(o => o.OrderSchedule)
                                        .ThenInclude(s => s.Assignments);
                        }
                        else
                        {
                                query = query
                                    .Include(o => o.OrderSchedule);
                        }
                }


                return await query.FirstOrDefaultAsync(o => o.Id == assignmentId);
        }


}