namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StudentContractRepository : IStudentContractRepository
{
        private readonly AppDbContext _context;

        public StudentContractRepository(AppDbContext context) => _context = context;

        public async Task<StudentContract> GetByIdAsync(int id)
            => await _context.StudentContracts
                .Include(sc => sc.Student)
                .FirstOrDefaultAsync(sc => sc.Id == id);

        public async Task<IEnumerable<StudentContract>> GetByStudentIdAsync(int studentId)
            => await _context.StudentContracts
                .Where(sc => sc.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<StudentContract>> GetActiveContracts(DateOnly date)
            => await _context.StudentContracts
                .Where(sc => sc.EffectiveDate <= date &&
                    (sc.ExpirationDate == null || sc.ExpirationDate >= date))
                .ToListAsync();

        public async Task<StudentContract> AddAsync(StudentContract contract)
        {
                await _context.StudentContracts.AddAsync(contract);
                await _context.SaveChangesAsync();
                return contract;
        }

        public async Task UpdateAsync(StudentContract contract)
        {
                _context.StudentContracts.Update(contract);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(StudentContract contract)
        {
                _context.StudentContracts.Remove(contract);
                await _context.SaveChangesAsync();
        }
}