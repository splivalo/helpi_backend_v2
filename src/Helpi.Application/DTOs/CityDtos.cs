using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;

public class CityDto
{
    public int Id { get; set; }
    public string OfficialName { get; set; } = null!;
    public bool IsServiced { get; set; }
}

public class CityCreateDto
{
    [Required]
    [StringLength(255)]
    public string GooglePlaceId { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string OfficialName { get; set; } = null!;

    [Required]
    public bool IsServiced { get; set; }
}

public class CityUpdateDto
{
    public bool? IsServiced { get; set; }
}