using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class FacultyDto
{
    public int Id { get; set; }
    public string FacultyName { get; set; } = null!;
}

public class FacultyCreateDto
{
    [Required]
    [StringLength(100)]
    public string FacultyName { get; set; } = null!;
}

public class FacultyUpdateDto : FacultyCreateDto { }