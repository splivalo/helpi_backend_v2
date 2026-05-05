namespace Helpi.Application.DTOs.JobInstance;

/// <summary>
/// Atomic reactivate + optional reschedule + optional student-change request.
/// Replaces the two-call pattern (reactivate → manage) that caused orphaned
/// "Rescheduled" sessions when both date and student changed simultaneously.
/// </summary>
public class ReactivateAndManageRequestDto
{
    /// <summary>New date for the session. Null = keep original.</summary>
    public DateOnly? NewDate { get; set; }

    /// <summary>New start time. Null = keep original.</summary>
    public TimeOnly? NewStartTime { get; set; }

    /// <summary>New end time. Null = keep original.</summary>
    public TimeOnly? NewEndTime { get; set; }

    /// <summary>
    /// Student to assign. Null = keep existing student.
    /// When provided a new <see cref="ScheduleAssignment"/> with
    /// <c>Status = PendingAcceptance</c> will be created and the student
    /// will be notified via SignalR / FCM.
    /// </summary>
    public int? PreferredStudentId { get; set; }
}
