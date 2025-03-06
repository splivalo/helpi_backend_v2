using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


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
    public JobRequestStatus? Status { get; set; }
}