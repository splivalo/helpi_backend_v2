using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(int id);
    Task<Coupon?> GetByIdWithAssignmentsAsync(int id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IEnumerable<Coupon>> GetAllAsync();
    Task<Coupon> AddAsync(Coupon coupon);
    Task UpdateAsync(Coupon coupon);
    Task DeleteAsync(Coupon coupon);

    // Assignments
    Task<CouponAssignment?> GetAssignmentByIdAsync(int id);
    Task<CouponAssignment?> GetActiveAssignmentAsync(int couponId, int seniorId);
    Task<List<CouponAssignment>> GetActiveAssignmentsForSeniorAsync(int seniorId);
    Task<CouponAssignment> AddAssignmentAsync(CouponAssignment assignment);
    Task UpdateAssignmentAsync(CouponAssignment assignment);
    Task<int> GetAssignmentCountAsync(int couponId);
    Task<List<CouponAssignment>> GetAssignmentsByCouponIdAsync(int couponId);

    // Usages
    Task<CouponUsage> AddUsageAsync(CouponUsage usage);
    Task<decimal> GetUsedHoursInPeriodAsync(int couponAssignmentId, DateTime periodStart, DateTime periodEnd);
}
