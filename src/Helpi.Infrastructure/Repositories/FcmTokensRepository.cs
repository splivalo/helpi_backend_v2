namespace Helpi.Infrastructure.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class FcmTokensRepository : IFcmTokensRepository
{
    private readonly AppDbContext _context;

    public FcmTokensRepository(AppDbContext context) => _context = context;

    public async Task<FcmToken> AddAsync(FcmToken fcmToken)
    {
        var existingToken = await _context.FcmTokens
            .FirstOrDefaultAsync(t => t.UserId == fcmToken.UserId && t.Token == fcmToken.Token);

        if (existingToken != null) return existingToken;

        await _context.FcmTokens.AddAsync(fcmToken);
        await _context.SaveChangesAsync();
        return fcmToken;
    }

    public async Task DeleteAsync(FcmToken fcmToken)
    {
        _context.FcmTokens.Remove(fcmToken);
        await _context.SaveChangesAsync();
    }

    public async Task<List<FcmToken>> GetTokensByUserIdAsync(int userId)
    {
        return await _context.FcmTokens.Where(t => t.UserId == userId).ToListAsync();
    }
}