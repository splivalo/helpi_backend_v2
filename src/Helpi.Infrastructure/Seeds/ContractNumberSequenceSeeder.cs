using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;




namespace Helpi.Infrastructure.Seeds
{
    public class ContractNumberSequenceSeeder
    {
        private readonly AppDbContext _context;

        public ContractNumberSequenceSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Check if ContractNumberSequences already exist in the database
            if (await _context.ContractNumberSequences.AnyAsync())
            {
                return; // Database already seeded
            }

            var first = new ContractNumberSequence(1, 1);

            // Add cities to the database
            await _context.ContractNumberSequences.AddAsync(first);
            await _context.SaveChangesAsync();
        }



    }
}