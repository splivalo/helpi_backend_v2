using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class JobRequest
    {
        public int Id { get; set; }
        public int OrderScheduleId { get; set; }
        public int? JobInstanceId { get; set; }
        public int StudentId { get; set; }
        public int SeniorId { get; set; }
        public int OrderId { get; set; }
        public JobRequestStatus Status { get; set; } = JobRequestStatus.Pending;
        public DateTime? RespondedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public byte PriorityLevel { get; set; } = 1;

        public OrderSchedule OrderSchedule { get; set; } = null!;
        public JobInstance? JobInstance { get; set; }
        public Student Student { get; set; } = null!;
        public Senior Senior { get; set; } = null!;
        public Order Order { get; set; } = null!;

        // Reassignment tracking
        public bool IsReassignment { get; set; } = false;
        public int? ReassignmentRecordId { get; set; }
        public ReassignmentType? ReassignmentType { get; set; }
        public int? ReassignAssignmentId { get; set; }
        public int? ReassignJobInstanceId { get; set; }
    }
}

/// TODO: SHOULD CANCEL JOBREQUESTS FOR ORDERS THAT ARE PAST DATE AND NOT ACCEPTED
/// TODO: IF A REQUEST IS ACCEPTED ON DAY OF ORDER , SCHULES STATUS UPDATES