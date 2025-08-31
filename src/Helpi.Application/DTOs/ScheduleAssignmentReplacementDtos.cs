// using System.ComponentModel.DataAnnotations;
// using Helpi.Domain.Enums;

// namespace Helpi.Application.DTOs;


// public class ScheduleAssignmentReplacementDto
// {
//     public int Id { get; set; }
//     public DateTime ReplacedAt { get; set; }
//     public ReplacementInitiator InitiatedBy { get; set; }
// }

// public class ScheduleAssignmentReplacementCreateDto
// {
//     [Required]
//     public int OriginalAssignmentId { get; set; }

//     [Required]
//     public int NewAssignmentId { get; set; }

//     [Required]
//     public ReplacementInitiator InitiatedBy { get; set; }

//     public string? ReplacementReason { get; set; }
// }