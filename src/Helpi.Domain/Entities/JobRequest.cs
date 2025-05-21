using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class JobRequest
    {
        public int Id { get; set; }
        public int OrderScheduleId { get; set; }
        public int StudentId { get; set; }
        public int SeniorId { get; set; }
        public int OrderId { get; set; }
        public JobRequestStatus Status { get; set; } = JobRequestStatus.Pending;
        public DateTime? RespondedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsEmergencySub { get; set; } = false;
        public byte PriorityLevel { get; set; } = 1;

        public OrderSchedule OrderSchedule { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Senior Senior { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}
