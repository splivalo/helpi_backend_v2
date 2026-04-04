using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IFacultyRepository
{
    Task<Faculty?> GetByIdAsync(int id);
    Task<Faculty?> GetByNameAsync(string name);
    Task<IEnumerable<Faculty>> GetAllAsync();
    Task<Faculty> AddAsync(Faculty faculty);
    Task UpdateAsync(Faculty faculty);
    Task DeleteAsync(Faculty faculty);
}
