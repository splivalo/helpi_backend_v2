

using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

/// <summary>
/// should only have 1 record (the active pricing config)
/// </summary>
public interface IPricingConfigurationRepository
{
    Task<PricingConfiguration?> GetByIdAsync(int id);
    Task<IEnumerable<PricingConfiguration>> GetAllAsync();
    Task AddAsync(PricingConfiguration config);
    Task UpdateAsync(PricingConfiguration config);
    Task DeleteAsync(int id);
}
