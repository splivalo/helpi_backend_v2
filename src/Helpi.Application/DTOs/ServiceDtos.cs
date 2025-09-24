using System.ComponentModel.DataAnnotations;
using Helpi.Domain.ValueObjects;

namespace Helpi.Application.DTOs;


public class ServiceDto
{
    public int Id { get; set; }
    public Dictionary<string, Translation> Translations { get; set; } = new();
    public int CategoryId { get; set; }

    public DateTime? DeletedOn { get; set; }
}

public class ServiceCreateDto
{
    [Required]
    public Dictionary<string, Translation> Translations { get; set; } = new();


    [Required]
    public int CategoryId { get; set; }


}

public class ServiceUpdateDto
{
    public Dictionary<string, Translation> Translations { get; set; } = new();
    public int? CategoryId { get; set; }

    public DateTime? DeletedOn { get; set; }
}