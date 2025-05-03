using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class ContactInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public Gender Gender { get; set; }
    public string GooglePlaceId { get; set; } = null!;
    public string FullAddress { get; set; } = null!;
    public int CityId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(2)]
    public string Country { get; set; } = "US";
}

public class ContactInfoCreateDto
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = null!;



    [Required]
    public Gender Gender { get; set; }

    [Required]
    public string GooglePlaceId { get; set; } = null!;

    [Required]
    public string FullAddress { get; set; } = null!;

    [Required]
    public int CityId { get; set; }
}

public class ContactInfoUpdateDto : ContactInfoCreateDto { }



