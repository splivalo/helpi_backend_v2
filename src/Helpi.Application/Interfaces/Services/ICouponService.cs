using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface ICouponService
{
    // CRUD
    Task<List<CouponDto>> GetAllAsync();
    Task<CouponDto?> GetByIdAsync(int id);
    Task<CouponDto> CreateAsync(CouponCreateDto dto);
    Task<CouponDto> UpdateAsync(int id, CouponUpdateDto dto);
    Task DeleteAsync(int id);

    // Assignments
    Task<CouponAssignmentDto> AssignToSeniorAsync(int couponId, int seniorId, int? adminId);
    Task<CouponRedeemResultDto> RedeemAsync(string code, int seniorId);
    Task<List<CouponAssignmentDto>> GetAssignmentsForSeniorAsync(int seniorId);
    Task<List<CouponAssignmentDto>> GetAssignmentsByCouponIdAsync(int couponId);
    Task DeactivateAssignmentAsync(int assignmentId);

    // Coverage calculation (for PaymentService integration)
    Task<CouponCoverageResultDto> CalculateCoverageAsync(int seniorId, decimal sessionHours, decimal totalAmount, decimal hourlyRate, int? cityId);
    Task RecordUsageAsync(int seniorId, int jobInstanceId, CouponCoverageResultDto coverage);

    // Order-level coupon validation & application (replaces PromoCode system)
    Task<CouponValidationResultDto> ValidateCodeForOrderAsync(string code, int seniorId, decimal orderTotal);
    Task<CouponValidationResultDto> ApplyToOrderAsync(string code, int orderId, int seniorId, decimal orderTotal);
}
