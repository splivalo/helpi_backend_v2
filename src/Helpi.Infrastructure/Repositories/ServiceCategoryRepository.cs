namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ServiceCategoryRepository : IServiceCategoryRepository
{
        private readonly AppDbContext _context;

        public ServiceCategoryRepository(AppDbContext context) => _context = context;

        public async Task<ServiceCategory> GetByIdAsync(int id)
            => await _context.ServiceCategories.FindAsync(id);

        public async Task<ServiceCategory> GetByNameAsync(string name)
            => await _context.ServiceCategories
                .FirstOrDefaultAsync(sc => sc.Name == name);

        public async Task<IEnumerable<ServiceCategory>> GetAllAsync()
            => await _context.ServiceCategories.ToListAsync();

        public async Task<ServiceCategory> AddAsync(ServiceCategory category)
        {
                await _context.ServiceCategories.AddAsync(category);
                await _context.SaveChangesAsync();
                return category;
        }

        public async Task UpdateAsync(ServiceCategory category)
        {
                _context.ServiceCategories.Update(category);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ServiceCategory category)
        {
                _context.ServiceCategories.Remove(category);
                await _context.SaveChangesAsync();
        }
}