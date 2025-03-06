using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public short MinDuration { get; set; }
    public int CategoryId { get; set; }
}

public class ServiceCreateDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Range(0, 999999.99)]
    public decimal BasePrice { get; set; }

    [Range(1, 32767)]
    public short MinDuration { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

public class ServiceUpdateDto
{
    [StringLength(255)]
    public string? Name { get; set; }
    public decimal? BasePrice { get; set; }
    public short? MinDuration { get; set; }
    public int? CategoryId { get; set; }
}