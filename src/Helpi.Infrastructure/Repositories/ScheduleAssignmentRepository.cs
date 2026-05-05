namespace Helpi.Infrastructure.Repositories;

using System.Runtime;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.EntityFrameworkCore;
public class ScheduleAssignmentRepository : IScheduleAssignmentRepository
{
        private readonly AppDbContext _context;

        public ScheduleAssignmentRepository(AppDbContext context) => _context = context;

        public async Task<ScheduleAssignment?> GetByIdAsync(int id)
        {

                return await _context.ScheduleAssignments
              .Include(sa => sa.OrderSchedule)
              .Include(sa => sa.Student).ThenInclude(s => s.Contracts)
              .FirstOrDefaultAsync(sa => sa.Id == id);
        }

        public async Task<IEnumerable<ScheduleAssignment>> GetByStudentAsync(int studentId)
            => await _context.ScheduleAssignments
                .Where(sa => sa.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<ScheduleAssignment>> GetByStudentWithDetailsAsync(int studentId)
            => await _context.ScheduleAssignments
                .Include(sa => sa.OrderSchedule)
                    .ThenInclude(os => os.Order)
                        .ThenInclude(o => o.Senior)
                            .ThenInclude(s => s.Contact)
                .Where(sa => sa.StudentId == studentId)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

        public async Task<List<ScheduleAssignment>> GetAssignmentsNeedingJobGenerationAsync()
        {

                var liveOrderStatuses = new[] {
                        OrderStatus.Pending,
                        OrderStatus.FullAssigned
                };

                // Step 1: Get assignments with necessary includes
                var assignments = await _context.ScheduleAssignments
                    .Include(sa => sa.OrderSchedule)
                        .ThenInclude(os => os.Order)
                            .ThenInclude(o => o.Senior)
                    .Where(sa => sa.Status == AssignmentStatus.Accepted &&
                            !sa.IsJobInstanceSub &&
                            sa.OrderSchedule.Order.IsRecurring &&
                            liveOrderStatuses.Contains(sa.OrderSchedule.Order.Status))
                    .AsSplitQuery() // Important: prevents cartesian explosion
                    .ToListAsync();

                // Step 2: Load latest job instances separately
                if (assignments.Any())
                {
                        var assignmentIds = assignments.Select(a => a.Id).ToHashSet();

                        // Get only the latest job instance per assignment
                        var latestJobInstances = await _context.JobInstances
                            .Where(ji => ji.IsRescheduleVariant == false)
                            .Where(ji => ji.ScheduleAssignmentId.HasValue)
                            .Where(ji => assignmentIds.Contains(ji.ScheduleAssignmentId!.Value))
                            .GroupBy(ji => ji.ScheduleAssignmentId)
                            .Select(g => g.OrderByDescending(x => x.ScheduledDate).First())
                            .ToDictionaryAsync(ji => ji.ScheduleAssignmentId!.Value);

                        // Attach to assignments
                        foreach (var assignment in assignments)
                        {
                                if (latestJobInstances.TryGetValue(assignment.Id, out var latestJob))
                                {
                                        assignment.JobInstances = new List<JobInstance> { latestJob };
                                }
                                else
                                {
                                        assignment.JobInstances = new List<JobInstance>();
                                }
                        }
                }

                return assignments;


        }

        public async Task<List<ScheduleAssignment>> GetAssignmentsNeedingJobGenerationForStudentAsync(int studentId)
        {
                var liveOrderStatuses = new[] {
                        OrderStatus.Pending,
                        OrderStatus.FullAssigned
                };

                var assignments = await _context.ScheduleAssignments
                    .Include(sa => sa.OrderSchedule)
                        .ThenInclude(os => os.Order)
                            .ThenInclude(o => o.Senior)
                    .Where(sa => sa.StudentId == studentId &&
                            sa.Status == AssignmentStatus.Accepted &&
                            !sa.IsJobInstanceSub &&
                            sa.OrderSchedule.Order.IsRecurring &&
                            liveOrderStatuses.Contains(sa.OrderSchedule.Order.Status))
                    .AsSplitQuery()
                    .ToListAsync();

                if (assignments.Any())
                {
                        var assignmentIds = assignments.Select(a => a.Id).ToHashSet();

                        var latestJobInstances = await _context.JobInstances
                            .Where(ji => ji.IsRescheduleVariant == false)
                            .Where(ji => ji.ScheduleAssignmentId.HasValue)
                            .Where(ji => assignmentIds.Contains(ji.ScheduleAssignmentId!.Value))
                            .GroupBy(ji => ji.ScheduleAssignmentId)
                            .Select(g => g.OrderByDescending(x => x.ScheduledDate).First())
                            .ToDictionaryAsync(ji => ji.ScheduleAssignmentId!.Value);

                        foreach (var assignment in assignments)
                        {
                                if (latestJobInstances.TryGetValue(assignment.Id, out var latestJob))
                                {
                                        assignment.JobInstances = new List<JobInstance> { latestJob };
                                }
                                else
                                {
                                        assignment.JobInstances = new List<JobInstance>();
                                }
                        }
                }

                return assignments;
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
                    .Where(sa => !sa.IsJobInstanceSub)
                    .OrderByDescending(sa => sa.AcceptedAt) // Get most recent
                    .Include(sa => sa.Student).ThenInclude(s => s.Contact)
                    .Select(sa => sa.Student)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
        }
        public async Task<ScheduleAssignment?> GetAssignmentForOrderScheduleAsync(int orderScheduleId)
        {
                var activeStatuses = new[]
               {
                        AssignmentStatus.Completed,
                        AssignmentStatus.Accepted,
                        AssignmentStatus.PendingAcceptance
                };

                return await _context.ScheduleAssignments
                    .Where(sa => sa.OrderScheduleId == orderScheduleId)
                    .Where(sa => activeStatuses.Contains(sa.Status))
                    .Where(sa => !sa.IsJobInstanceSub)
                    .OrderByDescending(sa => sa.AssignedAt) // Get most recent
                    .Include(sa => sa.Student).ThenInclude(s => s.Contact)
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
                    .AnyAsync(sa => sa.OrderScheduleId == scheduleId
                    && activeStatuses.Contains(sa.Status)
                    && !sa.IsJobInstanceSub);
        }


        public async Task<bool> IsAllOrderAssignmentsCompleted(int orderId)
        {
                var hasActiveAssignments = await _context.ScheduleAssignments.AnyAsync(a =>
                        a.OrderId == orderId &&
                        a.Status != AssignmentStatus.Completed &&
                        a.Status != AssignmentStatus.Terminated
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

        public async Task<ScheduleAssignment?> LoadAssignmentWithIncludes(
    int assignmentId,
    AssignmentIncludeOptions options)
        {
                var query = _context.ScheduleAssignments.AsQueryable();

                if (options.IncludeStudent)
                {

                        query = query.Include(o => o.Student)
                                     .ThenInclude(s => s.Contact);

                        if (options.IncludeStudentContracts)
                        {
                                query = query.Include(o => o.Student)
                                   .ThenInclude(s => s.Contracts);
                        }

                }

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
                                query = query.Include(o => o.OrderSchedule);
                        }

                        if (options.IncludeOrder)
                        {
                                query = query
                                    .Include(o => o.OrderSchedule)
                                        .ThenInclude(s => s.Order);
                        }
                }

                return await query.FirstOrDefaultAsync(o => o.Id == assignmentId);
        }


        public async Task<List<ScheduleAssignment>> GetActiveAssignmentsByStudentId(int studentId)
        {
                var activeStatuses = new[] { AssignmentStatus.Accepted, AssignmentStatus.PendingAcceptance };
                return await _context.ScheduleAssignments
                                .Include(a => a.OrderSchedule)
                                    .ThenInclude(os => os.Order)
                                .Where(a => a.StudentId == studentId
                                    && activeStatuses.Contains(a.Status)
                                    && a.OrderSchedule.Order.Status != OrderStatus.Completed
                                    && a.OrderSchedule.Order.Status != OrderStatus.Cancelled)
                                .ToListAsync();
        }

        public async Task<bool> HasActiveAssignmentsForServicesAsync(int studentId, List<int> serviceIds)
        {
                return await _context.ScheduleAssignments
                    .Where(sa => sa.StudentId == studentId &&
                                 sa.Status == AssignmentStatus.Accepted)
                    .SelectMany(sa => sa.OrderSchedule.Order.OrderServices)
                    .AnyAsync(os => serviceIds.Contains(os.ServiceId));
        }

        public async Task<bool> HasActiveAssignmentsForSlotsAsync(int studentId, List<StudentAvailabilitySlotCreateDto> dtos)
        {
                if (dtos == null || dtos.Count == 0) return false;

                var daysOfWeek = dtos.Select(d => d.DayOfWeek).Distinct();

                return await _context.ScheduleAssignments
                    .AnyAsync(sa => sa.StudentId == studentId &&
                                   daysOfWeek.Contains(sa.OrderSchedule.DayOfWeek) &&
                                   sa.Status == AssignmentStatus.Accepted);
        }

        public async Task<List<ScheduleAssignment>> GetConflictingAssignmentsAsync(int studentId, List<byte> removedDays)
        {
                if (removedDays == null || removedDays.Count == 0) return new List<ScheduleAssignment>();

                return await _context.ScheduleAssignments
                    .Include(sa => sa.OrderSchedule)
                        .ThenInclude(os => os.Order)
                            .ThenInclude(o => o.Senior)
                                .ThenInclude(s => s.Contact)
                    .Include(sa => sa.JobInstances)
                    .Where(sa => sa.StudentId == studentId &&
                                 removedDays.Contains(sa.OrderSchedule.DayOfWeek) &&
                                 sa.Status == AssignmentStatus.Accepted)
                    .ToListAsync();
        }

        public async Task<List<ScheduleAssignment>> GetAllPendingAcceptanceAsync()
        {
                return await _context.ScheduleAssignments
                    .Include(sa => sa.Student).ThenInclude(s => s.Contact)
                    .Include(sa => sa.OrderSchedule)
                        .ThenInclude(os => os.Order)
                            .ThenInclude(o => o.Senior)
                                .ThenInclude(s => s.Contact)
                    .Include(sa => sa.JobInstances)
                    .Where(sa => sa.Status == AssignmentStatus.PendingAcceptance)
                    .Where(sa =>
                        // Regular assignment: order still awaiting full assignment
                        (!sa.IsJobInstanceSub && sa.OrderSchedule.Order.Status == OrderStatus.Pending)
                        ||
                        // Per-session (reactivated) assignment: always show regardless of order status
                        sa.IsJobInstanceSub)
                    .OrderBy(sa => sa.AssignedAt)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync();
        }

        public async Task<List<ScheduleAssignment>> GetActiveSubAssignmentsForJobInstanceAsync(int jobInstanceId)
        {
                // Active sub-assignments currently attached to this JI, OR sub-assignments
                // referenced by JIs that share the same (OrderScheduleId, ScheduledDate)
                // — i.e. earlier sub assignments for the same calendar slot that were
                // detached on a later cancel without being terminated.
                var slotInfo = await _context.JobInstances
                        .Where(ji => ji.Id == jobInstanceId)
                        .Select(ji => new { ji.OrderScheduleId, ji.ScheduledDate })
                        .FirstOrDefaultAsync();

                if (slotInfo == null) return new List<ScheduleAssignment>();

                var activeStatuses = new[]
                {
                        AssignmentStatus.PendingAcceptance,
                        AssignmentStatus.Accepted,
                };

                // Collect candidate assignment IDs from (a) JIs in the same slot and
                // (b) any sub-assignment whose JobInstances reference this exact JI.
                var candidateIds = await _context.JobInstances
                        .Where(ji => ji.OrderScheduleId == slotInfo.OrderScheduleId
                                  && ji.ScheduledDate == slotInfo.ScheduledDate
                                  && ji.ScheduleAssignmentId.HasValue)
                        .Select(ji => ji.ScheduleAssignmentId!.Value)
                        .ToListAsync();

                if (candidateIds.Count == 0) return new List<ScheduleAssignment>();

                return await _context.ScheduleAssignments
                        .Where(sa => candidateIds.Contains(sa.Id)
                                  && sa.IsJobInstanceSub
                                  && activeStatuses.Contains(sa.Status))
                        .Include(sa => sa.Student).ThenInclude(s => s.Contact)
                        .ToListAsync();
        }

        public void Detach(ScheduleAssignment assignment)
        {
                _context.DetachEntity(assignment);
        }
}

