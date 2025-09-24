using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services
{

    public class ContractNumberService : IContractNumberService
    {
        private readonly IContractNumberSequenceRepository _repository;


        public ContractNumberService(IContractNumberSequenceRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> GetNextContractNumberAsync()
        {
            // Try the atomic operation first
            try
            {
                return await _repository.GetAndIncrementNumberAsync();
            }
            catch (Exception)
            {
                // Fallback approach if no sequence exists yet
                var sequence = await _repository.GetSequenceAsync();

                if (sequence == null)
                {
                    sequence = new ContractNumberSequence();
                    await _repository.SaveAsync(sequence);
                }

                int nextNumber = sequence.GetNextNumber();
                await _repository.SaveAsync(sequence);

                return nextNumber;
            }
        }
    }

}