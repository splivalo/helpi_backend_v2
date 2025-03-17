using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int CategoryId { get; set; }
}

public class ServiceCreateDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;


    [Required]
    public int CategoryId { get; set; }
}

public class ServiceUpdateDto
{
    [StringLength(255)]
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
}