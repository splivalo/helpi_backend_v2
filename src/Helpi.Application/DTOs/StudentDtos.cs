using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class StudentDto
{
    public int UserId { get; set; }
    public string StudentNumber { get; set; } = null!;
    public int FacultyId { get; set; }
    public DateTime DateRegistered { get; set; }
    public StudentStatus VerificationStatus { get; set; }
    public decimal AverageRating { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;
}

public class StudentCreateDto
{
    [Required]
    [StringLength(20)]
    public string StudentNumber { get; set; } = null!;

    [Required]
    public int FacultyId { get; set; }

    [Required]
    public int ContactId { get; set; }
}

public class StudentUpdateDto
{
    public StudentStatus? VerificationStatus { get; set; }
    public DateOnly? BackgroundCheckDate { get; set; }
}