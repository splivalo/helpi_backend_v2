using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Entities;
using Helpi.Domain.ValueObjects;

namespace Helpi.Application.DTOs;

public class ServiceCategoryDto
{
    public int Id { get; set; }
    public Dictionary<string, Translation> Translations { get; set; } = new();
    public string? Icon { get; set; } = "assets/images/pets.svg";

    public ICollection<ServiceDto> Services { get; set; } = new List<ServiceDto>();

    public DateTime? DeletedOn { get; set; }
}

public class ServiceCategoryCreateDto
{
    [Required]
    public Dictionary<string, Translation> Translations { get; set; } = new();


}

public class ServiceCategoryUpdateDto
{
    public Dictionary<string, Translation>? Translations { get; set; } = new();
    public DateTime? DeletedOn { get; set; }
}
