using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(int id);
    Task<IEnumerable<Service>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Service>> SearchAsync(string searchTerm);
    Task<Service> AddAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(Service service);
}