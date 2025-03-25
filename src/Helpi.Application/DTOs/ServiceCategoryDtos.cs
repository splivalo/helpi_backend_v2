using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Entities;

namespace Helpi.Application.DTOs;

public class ServiceCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }

    public ICollection<ServiceDto> Services { get; set; } = new List<ServiceDto>();
}

public class ServiceCategoryCreateDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Icon { get; set; }
}

public class ServiceCategoryUpdateDto : ServiceCategoryCreateDto { }