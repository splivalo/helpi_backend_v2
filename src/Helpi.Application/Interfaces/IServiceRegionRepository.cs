using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IServiceRegionRepository
{
    Task<ServiceRegion?> GetByIdAsync(int id);
    Task<IEnumerable<ServiceRegion>> GetByCityAsync(int cityId);
    Task<IEnumerable<ServiceRegion>> GetByServiceAsync(int serviceId);
    Task<ServiceRegion> AddAsync(ServiceRegion region);
    Task UpdateAsync(ServiceRegion region);
    Task DeleteAsync(ServiceRegion region);
}
