using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces.Services
{
    public interface IReassignmentService
    {
        Task<ReassignmentRecord> InitiateReassignment(
            ReassignmentType reassignmentType,
            ReassignmentTrigger trigger,
            string reason,
            int requestedByUserId,
            int? jobInstanceId = null,
            int? scheduleAssignmentId = null,
            int? preferedStudentId = null
            );

        Task CompleteReassignment(int reassignmentRecordId, int newStudentId);
        Task ReassignJobInstance(int jobInstanceId, ReassignmentType reassignmentType, string reason);
        Task ReassignAssignment(int assignmentId, ReassignmentType reassignmentType, string reason);
    }
}