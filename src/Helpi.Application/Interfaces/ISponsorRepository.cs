using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface ISponsorRepository
{
    Task<Sponsor?> GetByIdAsync(int id);
    Task<IEnumerable<Sponsor>> GetAllAsync();
    Task<IEnumerable<Sponsor>> GetActiveAsync();
    Task<Sponsor> AddAsync(Sponsor sponsor);
    Task UpdateAsync(Sponsor sponsor);
    Task DeleteAsync(Sponsor sponsor);
}
