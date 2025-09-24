

using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IPricingChangeHistoryRepository
{
    Task<IEnumerable<PricingChangeHistory>> GetAllAsync();
    Task<IEnumerable<PricingChangeHistory>> GetByConfigurationIdAsync(int configId);
    Task AddAsync(PricingChangeHistory history);
}
