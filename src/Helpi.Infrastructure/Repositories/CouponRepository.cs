using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _context;

    public CouponRepository(AppDbContext context) => _context = context;

    public async Task<Coupon?> GetByIdAsync(int id)
        => await _context.Coupons
            .Include(c => c.City)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Coupon?> GetByIdWithAssignmentsAsync(int id)
        => await _context.Coupons
            .Include(c => c.City)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Coupon?> GetByCodeAsync(string code)
        => await _context.Coupons
            .Include(c => c.City)
            .FirstOrDefaultAsync(c => c.Code == code);

    public async Task<IEnumerable<Coupon>> GetAllAsync()
        => await _context.Coupons
            .Include(c => c.City)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Coupon> AddAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
        await _context.SaveChangesAsync();
        return coupon;
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Coupon coupon)
    {
        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
    }

    // Assignments

    public async Task<CouponAssignment?> GetAssignmentByIdAsync(int id)
        => await _context.CouponAssignments
            .Include(a => a.Coupon)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<CouponAssignment?> GetActiveAssignmentAsync(int couponId, int seniorId)
        => await _context.CouponAssignments
            .FirstOrDefaultAsync(a => a.CouponId == couponId && a.SeniorId == seniorId && a.IsActive);

    public async Task<List<CouponAssignment>> GetActiveAssignmentsForSeniorAsync(int seniorId)
        => await _context.CouponAssignments
            .Include(a => a.Coupon)
            .Where(a => a.SeniorId == seniorId && a.IsActive)
            .ToListAsync();

    public async Task<CouponAssignment> AddAssignmentAsync(CouponAssignment assignment)
    {
        await _context.CouponAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAssignmentAsync(CouponAssignment assignment)
    {
        _context.CouponAssignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetAssignmentCountAsync(int couponId)
        => await _context.CouponAssignments
            .CountAsync(a => a.CouponId == couponId && a.IsActive);

    public async Task<List<CouponAssignment>> GetAssignmentsByCouponIdAsync(int couponId)
        => await _context.CouponAssignments
            .Include(a => a.Senior)
                .ThenInclude(s => s.Customer)
                    .ThenInclude(c => c.Contact)
            .Where(a => a.CouponId == couponId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

    // Usages

    public async Task<CouponUsage> AddUsageAsync(CouponUsage usage)
    {
        await _context.CouponUsages.AddAsync(usage);
        await _context.SaveChangesAsync();
        return usage;
    }

    public async Task<decimal> GetUsedHoursInPeriodAsync(int couponAssignmentId, DateTime periodStart, DateTime periodEnd)
        => await _context.CouponUsages
            .Where(u => u.CouponAssignmentId == couponAssignmentId
                && u.UsedAt >= periodStart
                && u.UsedAt < periodEnd
                && u.CoveredHours.HasValue)
            .SumAsync(u => u.CoveredHours!.Value);
}
