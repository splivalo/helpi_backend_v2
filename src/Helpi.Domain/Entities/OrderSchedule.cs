using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;


namespace Helpi.Domain.Entities
{

    public class OrderSchedule
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsCancelled { get; set; } = false;

        public int AutoScheduleAttemptCount { get; set; } = 0;

        public bool AllowAutoScheduling { get; set; } = true;

        public AutoScheduleDisableReason? AutoScheduleDisableReason { get; set; }

        [Column(TypeName = "text")]
        public string? CancellationReason { get; set; }



        public Order Order { get; set; } = null!;


        public ICollection<JobRequest> JobRequests { get; set; } = new List<JobRequest>();
        public ICollection<ScheduleAssignment> Assignments { get; set; } = new List<ScheduleAssignment>();


    }
}
