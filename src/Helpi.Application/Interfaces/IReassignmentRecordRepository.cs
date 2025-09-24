using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces
{
    public interface IReassignmentRecordRepository
    {
        Task<ReassignmentRecord?> GetByIdAsync(int id, ReassignmentIncludeOptions options, bool asNoTracking = true);
        Task<IEnumerable<ReassignmentRecord>> GetByOrderScheduleIdAsync(int orderScheduleId);
        Task<IEnumerable<ReassignmentRecord>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<ReassignmentRecord>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<ReassignmentRecord>> GetActiveReassignmentsAsync();
        Task<ReassignmentRecord?> GetScheduleActiveReassignmentAsync(int scheduleId);
        Task<IEnumerable<ReassignmentRecord>> GetExpiredReassignmentsAsync();
        Task<IEnumerable<ReassignmentRecord>> GetByStatusAsync(ReassignmentStatus status);
        Task<ReassignmentRecord> AddAsync(ReassignmentRecord record);
        Task UpdateAsync(ReassignmentRecord record);
        Task DeleteAsync(ReassignmentRecord record);
        Task<int> GetCountByStatusAsync(ReassignmentStatus status);
        Task<IEnumerable<ReassignmentRecord>> GetRecordsNeedingAttentionAsync();
        Task<bool> HasActiveReassignmentsForAssignmentAsync(int assignmentId);
        Task<bool> HasActiveReassignmentsForInstanceAsync(int instanceId);

        void Detach(ReassignmentRecord record);
    }
}