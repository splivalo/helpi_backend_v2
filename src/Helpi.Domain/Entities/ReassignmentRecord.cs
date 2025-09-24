using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

public class ReassignmentRecord
{
    public int Id { get; set; }

    // What's being reassigned
    public int? ReassignJobInstanceId { get; set; }
    public int? ReassignAssignmentId { get; set; }
    public int CurrentAssignmentId { get; set; }
    public int OrderScheduleId { get; set; }
    public int OrderId { get; set; }

    // Reassignment details
    public ReassignmentType ReassignmentType { get; set; }
    public ReassignmentTrigger Trigger { get; set; }
    public string Reason { get; set; } = string.Empty;

    // Status tracking
    public bool AllowAutoScheduling { get; set; } = true;
    public ReassignmentStatus Status { get; set; } = ReassignmentStatus.Requested;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Who's involved
    public int RequestedByUserId { get; set; } = 0; // Could be system (0), admin, or student
    public int? OriginalStudentId { get; set; }
    public int? NewStudentId { get; set; }

    // Attempt tracking
    public int AttemptCount { get; set; } = 0;
    public int MaxAttempts { get; set; } = 300000; // ??
    public DateTime? LastAttemptAt { get; set; }


    //  preferred student tracking
    public int? PreferredStudentId { get; set; }


    // Relationship to job requests
    public ICollection<JobRequest> JobRequests { get; set; } = new List<JobRequest>();

    // Navigation properties
    public JobInstance? ReassignJobInstance { get; set; }
    public ScheduleAssignment? ReassignAssignment { get; set; }
    public OrderSchedule OrderSchedule { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public Student? OriginalStudent { get; set; }
    public Student? NewStudent { get; set; }
}