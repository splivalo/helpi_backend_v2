using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

public class PromoCodeRepository : IPromoCodeRepository
{
    private readonly AppDbContext _context;

    public PromoCodeRepository(AppDbContext context) => _context = context;

    public async Task<PromoCode?> GetByIdAsync(int id)
        => await _context.PromoCodes.FindAsync(id);

    public async Task<PromoCode?> GetByCodeAsync(string code)
        => await _context.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == code);

    public async Task<IEnumerable<PromoCode>> GetAllAsync()
        => await _context.PromoCodes
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<PromoCode> AddAsync(PromoCode promoCode)
    {
        await _context.PromoCodes.AddAsync(promoCode);
        await _context.SaveChangesAsync();
        return promoCode;
    }

    public async Task UpdateAsync(PromoCode promoCode)
    {
        _context.PromoCodes.Update(promoCode);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(PromoCode promoCode)
    {
        _context.PromoCodes.Remove(promoCode);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasCustomerUsedCodeAsync(int promoCodeId, int customerId)
        => await _context.PromoCodeUsages
            .AnyAsync(u => u.PromoCodeId == promoCodeId && u.CustomerId == customerId);

    public async Task<PromoCodeUsage> AddUsageAsync(PromoCodeUsage usage)
    {
        await _context.PromoCodeUsages.AddAsync(usage);
        await _context.SaveChangesAsync();
        return usage;
    }

    public async Task<IEnumerable<PromoCodeUsage>> GetUsagesByPromoCodeIdAsync(int promoCodeId)
        => await _context.PromoCodeUsages
            .Where(u => u.PromoCodeId == promoCodeId)
            .OrderByDescending(u => u.UsedAt)
            .ToListAsync();
}
