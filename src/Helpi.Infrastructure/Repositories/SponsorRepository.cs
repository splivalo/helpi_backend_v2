using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

public class SponsorRepository : ISponsorRepository
{
    private readonly AppDbContext _context;

    public SponsorRepository(AppDbContext context) => _context = context;

    public async Task<Sponsor?> GetByIdAsync(int id)
        => await _context.Sponsors.FindAsync(id);

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
        => await _context.Sponsors
            .OrderBy(s => s.DisplayOrder)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Sponsor>> GetActiveAsync()
        => await _context.Sponsors
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

    public async Task<Sponsor> AddAsync(Sponsor sponsor)
    {
        await _context.Sponsors.AddAsync(sponsor);
        await _context.SaveChangesAsync();
        return sponsor;
    }

    public async Task UpdateAsync(Sponsor sponsor)
    {
        _context.Sponsors.Update(sponsor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Sponsor sponsor)
    {
        _context.Sponsors.Remove(sponsor);
        await _context.SaveChangesAsync();
    }
}
