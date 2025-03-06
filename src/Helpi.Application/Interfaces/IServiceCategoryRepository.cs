using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IServiceCategoryRepository
{
    Task<ServiceCategory> GetByIdAsync(int id);
    Task<ServiceCategory> GetByNameAsync(string name);
    Task<IEnumerable<ServiceCategory>> GetAllAsync();
    Task<ServiceCategory> AddAsync(ServiceCategory category);
    Task UpdateAsync(ServiceCategory category);
    Task DeleteAsync(ServiceCategory category);
}