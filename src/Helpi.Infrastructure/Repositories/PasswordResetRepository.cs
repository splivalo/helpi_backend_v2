
using Helpi.Application.Interfaces;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetCode code)
        {
            await _context.PasswordResetCodes.AddAsync(code);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetCode?> GetValidCodeAsync(string email, string code)
        {
            return await _context.PasswordResetCodes
                .Where(c => c.Email == email && c.Code == code && !c.Used && c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task MarkAsUsedAsync(PasswordResetCode codeEntry)
        {
            codeEntry.Used = true;
            _context.PasswordResetCodes.Update(codeEntry);
            await _context.SaveChangesAsync();
        }
    }
}
