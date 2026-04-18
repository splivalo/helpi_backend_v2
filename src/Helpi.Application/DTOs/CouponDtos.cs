using System.ComponentModel.DataAnnotations;
using Helpi.Domain.Enums;

namespace Helpi.Application.DTOs;

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public bool IsCombainable { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public int AssignmentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CouponCreateDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public CouponType Type { get; set; }

    [Required]
    [Range(0.01, 100000)]
    public decimal Value { get; set; }

    public bool IsCombainable { get; set; }
    public int? CityId { get; set; }

    [Required]
    public DateOnly ValidFrom { get; set; }

    [Required]
    public DateOnly ValidUntil { get; set; }
}

public class CouponUpdateDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public decimal? Value { get; set; }
    public bool? IsCombainable { get; set; }
    public int? CityId { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool? IsActive { get; set; }
}

public class CouponAssignmentDto
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public string CouponCode { get; set; } = null!;
    public string CouponName { get; set; } = null!;
    public CouponType CouponType { get; set; }
    public decimal CouponValue { get; set; }
    public bool IsCombainable { get; set; }
    public int SeniorId { get; set; }
    public string? SeniorName { get; set; }
    public int? AssignedByAdminId { get; set; }
    public decimal? RemainingValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class CouponAssignDto
{
    [Required]
    public int SeniorId { get; set; }
}

public class CouponRedeemDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Code { get; set; } = null!;

    [Required]
    public int SeniorId { get; set; }
}

public class CouponRedeemResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public CouponAssignmentDto? Assignment { get; set; }
}

public class CouponCoverageResultDto
{
    public decimal OriginalAmount { get; set; }
    public decimal CoveredAmount { get; set; }
    public decimal ChargeableAmount { get; set; }
    public List<CouponCoverageDetailDto> UsedCoupons { get; set; } = new();
}

public class CouponCoverageDetailDto
{
    public int CouponAssignmentId { get; set; }
    public string CouponName { get; set; } = null!;
    public CouponType CouponType { get; set; }
    public decimal CoveredAmount { get; set; }
    public decimal? CoveredHours { get; set; }
}

public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public CouponDto? Coupon { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? AvailableHours { get; set; }
}

public class CouponUsageDto
{
    public int Id { get; set; }
    public int CouponAssignmentId { get; set; }
    public string CouponName { get; set; } = null!;
    public int JobInstanceId { get; set; }
    public decimal CoveredAmount { get; set; }
    public decimal? CoveredHours { get; set; }
    public DateTime UsedAt { get; set; }
}
