using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class ScheduleAssignmentDto
{
    public int Id { get; set; }
    public AssignmentStatus Status { get; set; }
    public bool IsTemporary { get; set; }

    public int? PrevAssignmentId { get; set; }
    public DateTime AssignedAt { get; set; }

    public StudentDto Student { get; set; } = null!;
}

public class ScheduleAssignmentCreateDto
{
    [Required]
    public int OrderScheduleId { get; set; }

    [Required]
    public int StudentId { get; set; }
}

public class ScheduleAssignmentUpdateDto
{
    public AssignmentStatus? Status { get; set; }
    public TerminationReason? TerminationReason { get; set; }
}