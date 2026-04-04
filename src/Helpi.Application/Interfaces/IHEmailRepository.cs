using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IHEmailRepository
{
    Task<HEmail?> GetByIdAsync(int id);
    Task<IEnumerable<HEmail>> GetFailedEmailsAsync();
    Task<HEmail> AddAsync(HEmail email);
    Task UpdateAsync(HEmail email);
    Task DeleteAsync(HEmail email);
}