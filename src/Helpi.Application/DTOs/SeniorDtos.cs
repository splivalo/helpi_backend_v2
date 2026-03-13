using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;


public class SeniorDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Relationship Relationship { get; set; }
    public JsonDocument? SpecialRequirements { get; set; }
    public ContactInfoDto Contact { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }

    // Review aggregation
    public int TotalReviews { get; set; }
    public int TotalRatingSum { get; set; }
    public double AverageRating { get; set; }

    ///
    // Order status information
    public List<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();
}

public class SeniorCreateDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int ContactId { get; set; }

    [Required]
    public Relationship Relationship { get; set; }

    public JsonDocument? SpecialRequirements { get; set; }
}

public class SeniorUpdateDto
{
    public Relationship? Relationship { get; set; }
    public JsonDocument? SpecialRequirements { get; set; }
}

// Filter Model
public class SeniorFilterDto
{
    public int? CityId { get; set; }
    public int? SeniorId { get; set; }
    public int? CustomerId { get; set; }
    public OrderStatus? OrderStatus { get; set; }
    public string? SearchText { get; set; }
}


