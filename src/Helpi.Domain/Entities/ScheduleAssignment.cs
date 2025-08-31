using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class ScheduleAssignment
    {
        public int Id { get; set; }
        public int OrderScheduleId { get; set; }
        public int OrderId { get; set; }


        public int StudentId { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Accepted;
        public bool IsTemporary { get; set; } = false; // one day substitution

        public int? OriginalAssignmentId { get; set; }
        public TerminationReason? TerminationReason { get; set; }
        public DateTime? TerminatedAt { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public OrderSchedule OrderSchedule { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public ICollection<JobInstance> JobInstances { get; set; } = new List<JobInstance>();
    }
}
