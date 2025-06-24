using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class StudentDto
{
    public int UserId { get; set; }
    public string StudentNumber { get; set; } = null!;
    public int FacultyId { get; set; }
    public DateTime DateRegistered { get; set; }
    public StudentStatus Status { get; set; }
    public decimal AverageRating { get; set; }

    public DateTime? DeletedOn { get; set; }

    public DateOnly? BackgroundCheckDate { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;
    public FacultyDto? Faculty { get; set; }
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
    public StudentStatus? Status { get; set; }
    public DateOnly? BackgroundCheckDate { get; set; }
}

public class AvailabilityCriteria
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}

public class StudentFilterDto
{
    public int? CityId { get; set; }
    public string? SearchText { get; set; }
    public List<int>? ServiceIds { get; set; }
    public StudentStatus? Status { get; set; }
    public int? FacultyId { get; set; }

    // Multiple availability filtering
    public List<AvailabilityCriteria>? AvailabilityCriteria { get; set; }
    public bool? HasAvailabilitySlots { get; set; }
    public bool MatchAllAvailability { get; set; } = false; // true = AND, false = OR

    // Additional filters
    public decimal? MinAverageRating { get; set; }
    public bool? BackgroundCheckCompleted { get; set; }
    public bool? IncludeDeleted { get; set; } = false;
}


