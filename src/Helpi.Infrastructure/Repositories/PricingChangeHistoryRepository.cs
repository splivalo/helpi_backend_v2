using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Helpi.Infrastructure.Repositories;

public class PricingChangeHistoryRepository : IPricingChangeHistoryRepository
{
    private readonly AppDbContext _context;

    public PricingChangeHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PricingChangeHistory>> GetAllAsync()
    {
        return await _context.PricingChangeHistories
            .OrderByDescending(h => h.ChangeDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PricingChangeHistory>> GetByConfigurationIdAsync(int configId)
    {
        return await _context.PricingChangeHistories
            .Where(h => h.PricingConfigurationId == configId)
            .OrderByDescending(h => h.ChangeDate)
            .ToListAsync();
    }

    public async Task AddAsync(PricingChangeHistory history)
    {
        _context.PricingChangeHistories.Add(history);
        await _context.SaveChangesAsync();
    }
}
