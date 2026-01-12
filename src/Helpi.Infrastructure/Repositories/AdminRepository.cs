using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;

    public AdminRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Admin?> GetAdminByIdAsync(int id)
    {
        return await _context.Admins
            .Include(a => a.Contact)
            .FirstOrDefaultAsync(a => a.UserId == id);
    }
}

