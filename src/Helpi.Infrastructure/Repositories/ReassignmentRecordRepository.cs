using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Repositories
{
    public class ReassignmentRecordRepository : IReassignmentRecordRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReassignmentRecordRepository> _logger;

        public ReassignmentRecordRepository(AppDbContext context, ILogger<ReassignmentRecordRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReassignmentRecord?> GetByIdAsync(int id, ReassignmentIncludeOptions options, bool asNoTracking = true)
        {
            var query = _context.ReassignmentRecords.AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (options.IncludeJobInstance)
                query = query.Include(r => r.ReassignJobInstance);

            if (options.IncludeScheduleAssignment)
                query = query.Include(r => r.ReassignAssignment);

            if (options.IncludeOrderSchedule)
                query = query.Include(r => r.OrderSchedule);

            if (options.IncludeOrder)
                query = query.Include(r => r.Order);

            if (options.IncludeRequestedByUser)
                query = query.Include(r => r.RequestedByUser);

            if (options.IncludeOriginalStudent)
                query = query.Include(r => r.OriginalStudent);

            if (options.IncludeNewStudent)
                query = query.Include(r => r.NewStudent);

            if (options.IncludeJobRequests)
                query = query.Include(r => r.JobRequests);

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }


        public async Task<IEnumerable<ReassignmentRecord>> GetByOrderScheduleIdAsync(int orderScheduleId)
        {
            return await _context.ReassignmentRecords
                .Where(r => r.OrderScheduleId == orderScheduleId)
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ReassignmentRecord>> GetByOrderIdAsync(int orderId)
        {
            return await _context.ReassignmentRecords
                .Where(r => r.OrderId == orderId)
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ReassignmentRecord>> GetByStudentIdAsync(int studentId)
        {
            return await _context.ReassignmentRecords
                .Where(r => r.OriginalStudentId == studentId || r.NewStudentId == studentId)
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ReassignmentRecord>> GetActiveReassignmentsAsync()
        {
            var activeStatuses = new[] { ReassignmentStatus.Requested, ReassignmentStatus.InProgress };
            return await _context.ReassignmentRecords
                .Where(r => activeStatuses.Contains(r.Status))
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ReassignmentRecord?> GetScheduleActiveReassignmentAsync(int scheduleId)
        {
            var activeStatuses = new[] { ReassignmentStatus.Requested, ReassignmentStatus.InProgress };
            return await _context.ReassignmentRecords
                .Where(r => activeStatuses.Contains(r.Status))
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReassignmentRecord>> GetExpiredReassignmentsAsync()
        {
            var expirationTime = DateTime.UtcNow.AddHours(-24); // Reassignments older than 24 hours
            return await _context.ReassignmentRecords
                .Where(r => r.Status == ReassignmentStatus.Requested && r.RequestedAt < expirationTime)
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ReassignmentRecord>> GetByStatusAsync(ReassignmentStatus status)
        {
            return await _context.ReassignmentRecords
                .Where(r => r.Status == status)
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ReassignmentRecord> AddAsync(ReassignmentRecord record)
        {
            await _context.ReassignmentRecords.AddAsync(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task UpdateAsync(ReassignmentRecord record)
        {
            _context.ReassignmentRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ReassignmentRecord record)
        {
            _context.ReassignmentRecords.Remove(record);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCountByStatusAsync(ReassignmentStatus status)
        {
            return await _context.ReassignmentRecords
                .Where(r => r.Status == status)
                .CountAsync();
        }

        public async Task<IEnumerable<ReassignmentRecord>> GetRecordsNeedingAttentionAsync()
        {
            var attentionStatuses = new[] { ReassignmentStatus.Failed, ReassignmentStatus.Expired };
            return await _context.ReassignmentRecords
                .Where(r => attentionStatuses.Contains(r.Status))
                .Include(r => r.ReassignJobInstance)
                .Include(r => r.ReassignAssignment)
                .Include(r => r.OrderSchedule)
                .Include(r => r.Order)
                .Include(r => r.RequestedByUser)
                .Include(r => r.OriginalStudent)
                .Include(r => r.NewStudent)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> HasActiveReassignmentsForAssignmentAsync(int assignmentId)
        {
            var activeStatuses = new[] { ReassignmentStatus.Requested, ReassignmentStatus.InProgress };
            return await _context.ReassignmentRecords
                .AnyAsync(r => r.ReassignAssignmentId == assignmentId && activeStatuses.Contains(r.Status));
        }

        public async Task<bool> HasActiveReassignmentsForInstanceAsync(int instanceId)
        {
            var activeStatuses = new[] { ReassignmentStatus.Requested, ReassignmentStatus.InProgress };
            return await _context.ReassignmentRecords
                .AnyAsync(r => r.ReassignJobInstanceId == instanceId && activeStatuses.Contains(r.Status));
        }

        public void Detach(ReassignmentRecord record)
        {
            _context.DetachEntity(record);
        }

    }
}