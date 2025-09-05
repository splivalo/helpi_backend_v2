using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class JobInstance
    {
        public int Id { get; set; }
        public int SeniorId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }

        public int? ContractId { get; set; }
        public int ScheduleAssignmentId { get; set; }
        public int? OriginalAssignmentId { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public JobInstanceStatus Status { get; set; } = JobInstanceStatus.Upcoming;
        public bool NeedsSubstitute { get; set; } = false;
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        // Money
        public decimal HourlyRate { get; set; }

        public decimal DurationHours => (decimal)(EndTime.ToTimeSpan() - StartTime.ToTimeSpan()).TotalHours;
        public decimal TotalAmount => DurationHours * HourlyRate;

        // Profit split amounts (calculated from configuration)
        public decimal CompanyPercentage { get; set; }
        public decimal ServiceProviderPercentage { get; set; }
        public decimal CompanyAmount => TotalAmount * (CompanyPercentage / 100);
        public decimal ServiceProviderAmount => TotalAmount * (ServiceProviderPercentage / 100);
        public ScheduleAssignment Assignment { get; set; } = null!;
        public ScheduleAssignment? OriginalAssignment { get; set; }
        public PaymentTransaction? PaymentTransaction { get; set; }
        public Review? Review { get; set; }
        public Senior Senior { get; set; } = null!;
        public Order Order { get; set; } = null!;



        // Track hangfire scheduling state
        public string? HangFireStartStatusJobId { get; set; }
        public string? HangFireEndStatusJobId { get; set; }
        public string? HangFirePaymentJobId { get; set; }
    }
}


