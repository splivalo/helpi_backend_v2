using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

public class SuspensionLogRepository : ISuspensionLogRepository
{
    private readonly AppDbContext _context;

    public SuspensionLogRepository(AppDbContext context) => _context = context;

    public async Task<List<SuspensionLog>> GetByUserIdAsync(int userId)
    {
        return await _context.SuspensionLogs
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SuspensionLog> AddAsync(SuspensionLog log)
    {
        await _context.SuspensionLogs.AddAsync(log);
        await _context.SaveChangesAsync();
        return log;
    }
}
