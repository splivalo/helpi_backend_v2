using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _repo;
    private readonly IMapper _mapper;

    public CouponService(ICouponRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    #region CRUD

    public async Task<List<CouponDto>> GetAllAsync()
    {
        var coupons = await _repo.GetAllAsync();
        var dtos = new List<CouponDto>();
        foreach (var coupon in coupons)
        {
            var dto = _mapper.Map<CouponDto>(coupon);
            dto.AssignmentCount = await _repo.GetAssignmentCountAsync(coupon.Id);
            dto.CityName = coupon.City?.Name;
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<CouponDto?> GetByIdAsync(int id)
    {
        var coupon = await _repo.GetByIdWithAssignmentsAsync(id);
        if (coupon == null) return null;

        var dto = _mapper.Map<CouponDto>(coupon);
        dto.AssignmentCount = coupon.Assignments.Count;
        dto.CityName = coupon.City?.Name;
        return dto;
    }

    public async Task<CouponDto> CreateAsync(CouponCreateDto dto)
    {
        var existing = await _repo.GetByCodeAsync(dto.Code.ToUpperInvariant());
        if (existing != null)
            throw new ArgumentException($"Coupon code '{dto.Code}' already exists.");

        if (dto.Type == CouponType.Percentage && dto.Value > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%.");

        if (dto.ValidUntil < dto.ValidFrom)
            throw new ArgumentException("ValidUntil must be after ValidFrom.");

        var entity = new Coupon
        {
            Code = dto.Code.ToUpperInvariant(),
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Value = dto.Value,
            IsCombainable = dto.IsCombainable,
            CityId = dto.CityId,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            IsActive = true
        };

        var created = await _repo.AddAsync(entity);
        return _mapper.Map<CouponDto>(created);
    }

    public async Task<CouponDto> UpdateAsync(int id, CouponUpdateDto dto)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Coupon with ID {id} not found.");

        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Value.HasValue)
        {
            if (entity.Type == CouponType.Percentage && dto.Value.Value > 100)
                throw new ArgumentException("Percentage discount cannot exceed 100%.");
            entity.Value = dto.Value.Value;
        }
        if (dto.IsCombainable.HasValue) entity.IsCombainable = dto.IsCombainable.Value;
        if (dto.CityId.HasValue) entity.CityId = dto.CityId.Value;
        if (dto.ValidFrom.HasValue) entity.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidUntil.HasValue) entity.ValidUntil = dto.ValidUntil.Value;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return _mapper.Map<CouponDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new ArgumentException($"Coupon with ID {id} not found.");
        await _repo.DeleteAsync(entity);
    }

    #endregion

    #region Assignments

    public async Task<CouponAssignmentDto> AssignToSeniorAsync(int couponId, int seniorId, int? adminId)
    {
        var coupon = await _repo.GetByIdAsync(couponId)
            ?? throw new ArgumentException($"Coupon with ID {couponId} not found.");

        ValidateCouponActive(coupon);

        var existingAssignment = await _repo.GetActiveAssignmentAsync(couponId, seniorId);
        if (existingAssignment != null)
            throw new ArgumentException("This senior already has an active assignment for this coupon.");

        // Exclusive check: if coupon is exclusive, deactivate any other exclusive
        if (!coupon.IsCombainable)
        {
            var activeAssignments = await _repo.GetActiveAssignmentsForSeniorAsync(seniorId);
            foreach (var existing in activeAssignments)
            {
                var existingCoupon = await _repo.GetByIdAsync(existing.CouponId);
                if (existingCoupon != null && !existingCoupon.IsCombainable)
                {
                    existing.IsActive = false;
                    await _repo.UpdateAssignmentAsync(existing);
                }
            }
        }

        decimal? remainingValue = IsHourBased(coupon.Type) ? coupon.Value : null;

        var assignment = new CouponAssignment
        {
            CouponId = couponId,
            SeniorId = seniorId,
            AssignedByAdminId = adminId,
            RemainingValue = remainingValue,
            IsActive = true
        };

        var created = await _repo.AddAssignmentAsync(assignment);
        return MapAssignmentDto(created, coupon);
    }

    public async Task<CouponRedeemResultDto> RedeemAsync(string code, int seniorId)
    {
        var coupon = await _repo.GetByCodeAsync(code.ToUpperInvariant());
        if (coupon == null)
            return new CouponRedeemResultDto { IsValid = false, ErrorCode = "coupon_not_found", ErrorMessage = "Coupon code not found." };

        var validationError = ValidateCouponForRedeem(coupon);
        if (validationError != null)
            return new CouponRedeemResultDto { IsValid = false, ErrorCode = validationError, ErrorMessage = validationError };

        var existingAssignment = await _repo.GetActiveAssignmentAsync(coupon.Id, seniorId);
        if (existingAssignment != null)
            return new CouponRedeemResultDto { IsValid = false, ErrorCode = "coupon_already_active", ErrorMessage = "You already have this coupon active." };

        // Exclusive check for senior self-redeem
        if (!coupon.IsCombainable)
        {
            var activeAssignments = await _repo.GetActiveAssignmentsForSeniorAsync(seniorId);
            var hasExclusive = false;
            foreach (var existing in activeAssignments)
            {
                var existingCoupon = await _repo.GetByIdAsync(existing.CouponId);
                if (existingCoupon != null && !existingCoupon.IsCombainable)
                {
                    hasExclusive = true;
                    break;
                }
            }
            if (hasExclusive)
                return new CouponRedeemResultDto { IsValid = false, ErrorCode = "exclusive_coupon_conflict", ErrorMessage = "You already have an active exclusive coupon. Contact Helpi for assistance." };
        }

        try
        {
            var assignment = await AssignToSeniorAsync(coupon.Id, seniorId, null);
            return new CouponRedeemResultDto { IsValid = true, Assignment = assignment };
        }
        catch (ArgumentException ex)
        {
            return new CouponRedeemResultDto { IsValid = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<List<CouponAssignmentDto>> GetAssignmentsForSeniorAsync(int seniorId)
    {
        var assignments = await _repo.GetActiveAssignmentsForSeniorAsync(seniorId);
        var dtos = new List<CouponAssignmentDto>();
        foreach (var a in assignments)
        {
            var coupon = await _repo.GetByIdAsync(a.CouponId);
            if (coupon != null)
                dtos.Add(MapAssignmentDto(a, coupon));
        }
        return dtos;
    }

    public async Task<List<CouponAssignmentDto>> GetAssignmentsByCouponIdAsync(int couponId)
    {
        var coupon = await _repo.GetByIdAsync(couponId)
            ?? throw new ArgumentException($"Coupon with ID {couponId} not found.");

        var assignments = await _repo.GetAssignmentsByCouponIdAsync(couponId);
        return assignments.Select(a => MapAssignmentDto(a, coupon)).ToList();
    }

    public async Task DeactivateAssignmentAsync(int assignmentId)
    {
        var assignment = await _repo.GetAssignmentByIdAsync(assignmentId)
            ?? throw new ArgumentException($"Assignment with ID {assignmentId} not found.");

        assignment.IsActive = false;
        await _repo.UpdateAssignmentAsync(assignment);
    }

    #endregion

    #region Coverage Calculation

    public async Task<CouponCoverageResultDto> CalculateCoverageAsync(
        int seniorId, decimal sessionHours, decimal totalAmount, decimal hourlyRate, int? cityId)
    {
        var result = new CouponCoverageResultDto
        {
            OriginalAmount = totalAmount,
            CoveredAmount = 0,
            ChargeableAmount = totalAmount
        };

        var assignments = await _repo.GetActiveAssignmentsForSeniorAsync(seniorId);
        if (assignments.Count == 0) return result;

        // Load coupons and filter valid ones
        var validAssignments = new List<(CouponAssignment Assignment, Coupon Coupon)>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var assignment in assignments)
        {
            var coupon = await _repo.GetByIdAsync(assignment.CouponId);
            if (coupon == null || !coupon.IsActive) continue;
            if (today < coupon.ValidFrom || today > coupon.ValidUntil) continue;

            // City check: null CityId on coupon = all cities
            if (coupon.CityId.HasValue && cityId.HasValue && coupon.CityId.Value != cityId.Value)
                continue;

            validAssignments.Add((assignment, coupon));
        }

        if (validAssignments.Count == 0) return result;

        // Check for exclusive coupons
        var exclusiveEntry = validAssignments.FirstOrDefault(v => !v.Coupon.IsCombainable);
        List<(CouponAssignment Assignment, Coupon Coupon)> applicableAssignments;

        if (exclusiveEntry.Coupon != null)
        {
            // Exclusive coupon found — use only this one
            applicableAssignments = new List<(CouponAssignment, Coupon)> { exclusiveEntry };
        }
        else
        {
            // All combinable — stack them
            applicableAssignments = validAssignments;
        }

        // Apply in priority order: hour-based → percentage → fixed
        var hourBased = applicableAssignments
            .Where(a => IsHourBased(a.Coupon.Type))
            .ToList();
        var percentageBased = applicableAssignments
            .Where(a => a.Coupon.Type == CouponType.Percentage)
            .ToList();
        var fixedBased = applicableAssignments
            .Where(a => a.Coupon.Type == CouponType.FixedPerSession)
            .ToList();

        decimal remainingAmount = totalAmount;
        decimal remainingHours = sessionHours;

        // 1. Hour-based coupons
        foreach (var (assignment, coupon) in hourBased)
        {
            if (remainingHours <= 0) break;

            var availableHours = await GetAvailableHoursAsync(assignment, coupon);
            if (availableHours <= 0) continue;

            var coveredHours = Math.Min(availableHours, remainingHours);
            var coveredAmount = Math.Round(coveredHours * hourlyRate, 2);
            coveredAmount = Math.Min(coveredAmount, remainingAmount);

            result.UsedCoupons.Add(new CouponCoverageDetailDto
            {
                CouponAssignmentId = assignment.Id,
                CouponName = coupon.Name,
                CouponType = coupon.Type,
                CoveredAmount = coveredAmount,
                CoveredHours = coveredHours
            });

            remainingAmount -= coveredAmount;
            remainingHours -= coveredHours;
        }

        // 2. Percentage coupons
        foreach (var (assignment, coupon) in percentageBased)
        {
            if (remainingAmount <= 0) break;

            var coveredAmount = Math.Round(remainingAmount * (coupon.Value / 100), 2);

            result.UsedCoupons.Add(new CouponCoverageDetailDto
            {
                CouponAssignmentId = assignment.Id,
                CouponName = coupon.Name,
                CouponType = coupon.Type,
                CoveredAmount = coveredAmount
            });

            remainingAmount -= coveredAmount;
        }

        // 3. Fixed per session coupons
        foreach (var (assignment, coupon) in fixedBased)
        {
            if (remainingAmount <= 0) break;

            var coveredAmount = Math.Min(coupon.Value, remainingAmount);

            result.UsedCoupons.Add(new CouponCoverageDetailDto
            {
                CouponAssignmentId = assignment.Id,
                CouponName = coupon.Name,
                CouponType = coupon.Type,
                CoveredAmount = coveredAmount
            });

            remainingAmount -= coveredAmount;
        }

        result.CoveredAmount = totalAmount - remainingAmount;
        result.ChargeableAmount = Math.Max(0, remainingAmount);

        return result;
    }

    public async Task RecordUsageAsync(int seniorId, int jobInstanceId, CouponCoverageResultDto coverage)
    {
        foreach (var detail in coverage.UsedCoupons)
        {
            var usage = new CouponUsage
            {
                CouponAssignmentId = detail.CouponAssignmentId,
                JobInstanceId = jobInstanceId,
                CoveredAmount = detail.CoveredAmount,
                CoveredHours = detail.CoveredHours
            };
            await _repo.AddUsageAsync(usage);

            // Update RemainingValue for one_time_hours
            var assignment = await _repo.GetAssignmentByIdAsync(detail.CouponAssignmentId);
            if (assignment != null && detail.CoveredHours.HasValue && assignment.RemainingValue.HasValue)
            {
                assignment.RemainingValue -= detail.CoveredHours.Value;
                if (assignment.RemainingValue <= 0)
                {
                    assignment.RemainingValue = 0;
                    assignment.IsActive = false;
                }
                await _repo.UpdateAssignmentAsync(assignment);
            }
        }
    }

    #endregion

    #region Private Helpers

    private async Task<decimal> GetAvailableHoursAsync(CouponAssignment assignment, Coupon coupon)
    {
        switch (coupon.Type)
        {
            case CouponType.OneTimeHours:
                return assignment.RemainingValue ?? 0;

            case CouponType.MonthlyHours:
                {
                    var now = DateTime.UtcNow;
                    var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1);
                    var used = await _repo.GetUsedHoursInPeriodAsync(assignment.Id, monthStart, monthEnd);
                    return Math.Max(0, coupon.Value - used);
                }

            case CouponType.WeeklyHours:
                {
                    var now = DateTime.UtcNow;
                    var daysToMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    var weekStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc)
                        .AddDays(-daysToMonday);
                    var weekEnd = weekStart.AddDays(7);
                    var used = await _repo.GetUsedHoursInPeriodAsync(assignment.Id, weekStart, weekEnd);
                    return Math.Max(0, coupon.Value - used);
                }

            default:
                return 0;
        }
    }

    private static bool IsHourBased(CouponType type)
    {
        return type == CouponType.MonthlyHours
            || type == CouponType.WeeklyHours
            || type == CouponType.OneTimeHours;
    }

    private static void ValidateCouponActive(Coupon coupon)
    {
        if (!coupon.IsActive)
            throw new ArgumentException("This coupon is not active.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today < coupon.ValidFrom)
            throw new ArgumentException("This coupon is not yet valid.");
        if (today > coupon.ValidUntil)
            throw new ArgumentException("This coupon has expired.");
    }

    private static string? ValidateCouponForRedeem(Coupon coupon)
    {
        if (!coupon.IsActive)
            return "coupon_inactive";

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today < coupon.ValidFrom)
            return "coupon_not_yet_valid";
        if (today > coupon.ValidUntil)
            return "coupon_expired";

        return null;
    }

    private static CouponAssignmentDto MapAssignmentDto(CouponAssignment assignment, Coupon coupon)
    {
        return new CouponAssignmentDto
        {
            Id = assignment.Id,
            CouponId = coupon.Id,
            CouponCode = coupon.Code,
            CouponName = coupon.Name,
            CouponType = coupon.Type,
            CouponValue = coupon.Value,
            IsCombainable = coupon.IsCombainable,
            SeniorId = assignment.SeniorId,
            SeniorName = assignment.Senior?.Customer?.Contact?.FullName,
            AssignedByAdminId = assignment.AssignedByAdminId,
            RemainingValue = assignment.RemainingValue,
            IsActive = assignment.IsActive,
            AssignedAt = assignment.AssignedAt
        };
    }

    #endregion
}
