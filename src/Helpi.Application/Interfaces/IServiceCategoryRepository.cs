using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IServiceCategoryRepository
{
    Task<ServiceCategory?> GetByIdAsync(int id);
    Task<IEnumerable<ServiceCategory>> GetAllAsync(bool excludeDeleted = true);
    Task<ServiceCategory> AddAsync(ServiceCategory category);
    Task UpdateAsync(ServiceCategory category);
    Task DeleteAsync(ServiceCategory category);

}