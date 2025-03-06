using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class JobInstance
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int? OriginalAssignmentId { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public JobInstanceStatus Status { get; set; } = JobInstanceStatus.Upcoming;
        public SubstitutionStatus SubstitutionStatus { get; set; } = SubstitutionStatus.Original;
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        public ScheduleAssignment Assignment { get; set; } = null!;
        public ScheduleAssignment? OriginalAssignment { get; set; }
        public PaymentTransaction? PaymentTransaction { get; set; }
        public Review? Review { get; set; }
    }
}
