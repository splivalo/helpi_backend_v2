using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;


public class ServiceRegionDto
{
    public int Id { get; set; }
    public bool Active { get; set; }
    public int CoverageRadiusKm { get; set; }
}

public class ServiceRegionCreateDto
{
    [Required]
    public int CityId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    public bool Active { get; set; }

    [Range(1, 100)]
    public int CoverageRadiusKm { get; set; }
}

public class ServiceRegionUpdateDto
{
    public bool? Active { get; set; }
    public int? CoverageRadiusKm { get; set; }
}