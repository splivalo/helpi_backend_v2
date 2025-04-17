using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class SeniorDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Relationship Relationship { get; set; }
    public JsonDocument? SpecialRequirements { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;
}

public class SeniorCreateDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int ContactId { get; set; }

    [Required]
    public Relationship Relationship { get; set; }

    public JsonDocument? SpecialRequirements { get; set; }
}

public class SeniorUpdateDto
{
    public Relationship? Relationship { get; set; }
    public JsonDocument? SpecialRequirements { get; set; }
}