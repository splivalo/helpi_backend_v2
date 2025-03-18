using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs.JobRequest;


public class JobRequestDto
{
    public int Id { get; set; }
    public JobRequestStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsEmergencySub { get; set; }
}

public class JobRequestCreateDto
{
    [Required]
    public int OrderScheduleId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public DateTime ExpiresAt { get; set; }
    public bool IsEmergencySub { get; set; }
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