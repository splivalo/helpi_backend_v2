using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class JobRequest
    {
        public int Id { get; set; }
        public int OrderScheduleId { get; set; }
        public int StudentId { get; set; }
        public JobRequestStatus Status { get; set; } = JobRequestStatus.Pending;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsEmergencySub { get; set; } = false;
        public byte PriorityLevel { get; set; } = 1;

        public OrderSchedule OrderSchedule { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
