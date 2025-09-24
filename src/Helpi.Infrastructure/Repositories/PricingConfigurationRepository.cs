using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Helpi.Infrastructure.Repositories;

public class PricingConfigurationRepository : IPricingConfigurationRepository
{
    private readonly AppDbContext _context;

    public PricingConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PricingConfiguration?> GetByIdAsync(int id)
    {
        return await _context.PricingConfigurations.FindAsync(id);
    }

    public async Task<IEnumerable<PricingConfiguration>> GetAllAsync()
    {
        return await _context.PricingConfigurations.ToListAsync();
    }

    public async Task AddAsync(PricingConfiguration config)
    {
        _context.PricingConfigurations.Add(config);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PricingConfiguration config)
    {
        _context.PricingConfigurations.Update(config);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var config = await _context.PricingConfigurations.FindAsync(id);
        if (config != null)
        {
            _context.PricingConfigurations.Remove(config);
            await _context.SaveChangesAsync();
        }
    }
}
