using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ContractNumberSequenceRepository : IContractNumberSequenceRepository
    {
        private readonly AppDbContext _context;

        public ContractNumberSequenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractNumberSequence> GetSequenceAsync()
        {
            return await _context.ContractNumberSequences.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Contract number sequence is not initialized.");
        }

        public async Task<int> GetAndIncrementNumberAsync()
        {
            //  distributed system  safe 
            var sql = @"
                UPDATE ContractNumberSequences 
                SET NextNumber = NextNumber + 1 
                OUTPUT INSERTED.NextNumber - 1 AS CurrentNumber
                WHERE Id = 1";

            var nextNumber = await _context.Database
                .SqlQueryRaw<int>(sql)
                .FirstOrDefaultAsync();

            return nextNumber;
        }

        public async Task SaveAsync(ContractNumberSequence sequence)
        {
            if (sequence.Id == 0)
                _context.ContractNumberSequences.Add(sequence);

            await _context.SaveChangesAsync();
        }
    }
}