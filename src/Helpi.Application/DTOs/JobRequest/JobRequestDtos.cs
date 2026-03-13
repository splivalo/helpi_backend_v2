using System.ComponentModel.DataAnnotations;
using Helpi.Application.DTOs.Order;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.JobRequest;


public class JobRequestDto
{
    public int Id { get; set; }

    public int OrderScheduleId { get; set; }
    public int? JobInstanceId { get; set; }

    public int OrderId { get; set; }
    public int SeniorId { get; set; }
    public int StudentId { get; set; }
    public JobRequestStatus Status { get; set; }
    public SessionDto? JobInstance { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Reassignment tracking
    public bool IsReassignment { get; set; } = false;
    public int? ReassignmentRecordId { get; set; }
    public ReassignmentType? ReassignmentType { get; set; }
    public int? ReassignAssignmentId { get; set; }
    public int? ReassignJobInstanceId { get; set; }

    public OrderScheduleDto OrderSchedule { get; set; } = null!;
    public OrderDto Order { get; set; } = null!;
    public SeniorDto Senior { get; set; } = null!;
}

public class JobRequestCreateDto
{
    [Required]
    public int OrderScheduleId { get; set; }

    [Required]
    public int OrderId { get; set; }


    [Required]
    public int SeniorId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public DateTime ExpiresAt { get; set; }
}

public class JobRequestUpdateDto
{


    [Required]
    public int JobRequestId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public JobRequestStatus? Status { get; set; }


    public string? RejectionReason { get; set; }
}

public class RespondToJobRequestDto
{


    [Required]
    public int JobRequestId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public bool isAccepted { get; set; }

    public string? RejectionReason { get; set; }
}