using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces
{
    public interface IContractNumberSequenceRepository
    {
        Task<ContractNumberSequence> GetSequenceAsync();
        Task<int> GetAndIncrementNumberAsync();
        Task SaveAsync(ContractNumberSequence sequence);
    }
}