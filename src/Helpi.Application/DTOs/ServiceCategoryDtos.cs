using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;

public class ServiceCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
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