namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
public class ServiceRegionRepository : IServiceRegionRepository
{
        private readonly AppDbContext _context;

        public ServiceRegionRepository(AppDbContext context) => _context = context;

        public async Task<ServiceRegion?> GetByIdAsync(int id)
    => await _context.ServiceRegions
        .Include(sr => sr.City)
        .Include(sr => sr.Service)
        .FirstOrDefaultAsync(sr => sr.Id == id);

        public async Task<IEnumerable<ServiceRegion>> GetByCityAsync(int cityId)
            => await _context.ServiceRegions
                .Where(sr => sr.CityId == cityId)
                .ToListAsync();

        public async Task<IEnumerable<ServiceRegion>> GetByServiceAsync(int serviceId)
            => await _context.ServiceRegions
                .Where(sr => sr.ServiceId == serviceId)
                .ToListAsync();

        public async Task<ServiceRegion> AddAsync(ServiceRegion region)
        {
                await _context.ServiceRegions.AddAsync(region);
                await _context.SaveChangesAsync();
                return region;
        }

        public async Task UpdateAsync(ServiceRegion region)
        {
                _context.ServiceRegions.Update(region);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ServiceRegion region)
        {
                _context.ServiceRegions.Remove(region);
                await _context.SaveChangesAsync();
        }
}