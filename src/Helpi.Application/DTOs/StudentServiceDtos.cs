using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class StudentServiceDto
{
    public int StudentId { get; set; }
    public int ServiceId { get; set; }
    public byte? ExperienceYears { get; set; }
}

public class StudentServiceCreateDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Range(0, 50)]
    public byte? ExperienceYears { get; set; }
}

public class StudentServiceUpdateDto
{
    [Range(0, 50)]
    public byte? ExperienceYears { get; set; }
}