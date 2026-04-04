namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ServiceRepository : IServiceRepository
{
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context) => _context = context;

        public async Task<Service?> GetByIdAsync(int id)
    => await _context.Services
        .Include(s => s.Category)
        .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<Service>> GetByCategoryAsync(int categoryId)
            => await _context.Services
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();

        public async Task<IEnumerable<Service>> SearchAsync(string searchTerm)
        {
                return await _context.Services.ToListAsync();
        }

        public async Task<Service> AddAsync(Service service)
        {
                await _context.Services.AddAsync(service);
                await _context.SaveChangesAsync();
                return service;
        }

        public async Task UpdateAsync(Service service)
        {
                _context.Services.Update(service);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Service service)
        {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
        }
}