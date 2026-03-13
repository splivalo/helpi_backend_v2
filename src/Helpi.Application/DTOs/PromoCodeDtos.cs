using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class PromoCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public PromoCodeType Type { get; set; }
    public decimal DiscountValue { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PromoCodeCreateDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Code { get; set; } = null!;

    [Required]
    public PromoCodeType Type { get; set; }

    [Required]
    [Range(0.01, 100000)]
    public decimal DiscountValue { get; set; }

    public int? MaxUses { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
}

public class PromoCodeUpdateDto
{
    public decimal? DiscountValue { get; set; }
    public int? MaxUses { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool? IsActive { get; set; }
}

public class PromoCodeValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public PromoCodeDto? PromoCode { get; set; }
    public decimal DiscountAmount { get; set; }
}

public class PromoCodeUsageDto
{
    public int Id { get; set; }
    public int PromoCodeId { get; set; }
    public string PromoCodeCode { get; set; } = null!;
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime UsedAt { get; set; }
}
