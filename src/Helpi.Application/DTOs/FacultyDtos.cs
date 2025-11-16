using System.ComponentModel.DataAnnotations;
using Helpi.Domain.ValueObjects;

namespace Helpi.Application.DTOs;


public class FacultyDto
{
    public int Id { get; set; }
    public Dictionary<string, Translation> Translations { get; set; } = new();
}

public class FacultyCreateDto
{
    [Required]
    public Dictionary<string, Translation> Translations { get; set; } = new();
}

public class FacultyUpdateDto : FacultyCreateDto { }