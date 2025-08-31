using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class JobInstanceDto
{
    public int Id { get; set; }

    public int? ContractId { get; set; }
    public int ScheduleAssignmentId { get; set; }
    public int? OriginalAssignmentId { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public JobInstanceStatus Status { get; set; }

    public SeniorDto Senior { get; set; } = null!;

    public ScheduleAssignmentDto assignment { get; set; } = null!;
}

public class JobInstanceUpdateDto
{
    public JobInstanceStatus? Status { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
}