using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;

public class StudentContractDto
{
    public int Id { get; set; }
    public string CloudPath { get; set; } = null!;
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
}

public class StudentContractCreateDto
{
    [Required]
    [Url]
    [StringLength(512)]
    public string CloudPath { get; set; } = null!;

    [Required]
    public DateOnly EffectiveDate { get; set; }

    public DateOnly? ExpirationDate { get; set; }
}

public class StudentContractUpdateDto
{
    [Url]
    [StringLength(512)]
    public string? CloudPath { get; set; }
    public DateOnly? ExpirationDate { get; set; }
}