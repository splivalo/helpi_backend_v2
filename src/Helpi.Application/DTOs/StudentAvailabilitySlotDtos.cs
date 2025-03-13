using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class StudentAvailabilitySlotDto
{


    public int StudentId { get; set; }
    public byte DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public class StudentAvailabilitySlotCreateDto
{
    [Range(0, 6)]
    public byte DayOfWeek { get; set; }

    public int StudentId { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}

public class StudentAvailabilitySlotUpdateDto : StudentAvailabilitySlotCreateDto { }