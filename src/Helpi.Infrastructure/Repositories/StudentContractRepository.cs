namespace Helpi.Infrastructure.Repositories;

using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StudentContractRepository : IStudentContractRepository
{
    private readonly AppDbContext _context;

    public StudentContractRepository(AppDbContext context) => _context = context;

    public async Task<StudentContract?> GetByIdAsync(int id)
        => await _context.StudentContracts
            .Where(c => c.DeletedOn == null)
            .Include(sc => sc.Student)
            .SingleOrDefaultAsync(sc => sc.Id == id);

    public async Task<IEnumerable<StudentContract>> GetByStudentIdAsync(int studentId)
        => await _context.StudentContracts
          .Where(c => c.DeletedOn == null)
            .Where(sc => sc.StudentId == studentId)
             .OrderByDescending(sc => sc.ExpirationDate)
            .ToListAsync();

    public async Task<IEnumerable<StudentContract>> GetActiveContracts(DateOnly date)
        => await _context.StudentContracts
          .Where(c => c.DeletedOn == null)
            .Where(sc => sc.EffectiveDate <= date &&
                (sc.ExpirationDate >= date))
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

    public Task<int> CountAsync(Expression<Func<StudentContract, bool>> predicate)
=> _context.StudentContracts
  .Where(c => c.DeletedOn == null)
.CountAsync(predicate);

    public async Task<List<StudentContract>> GetCompletedContractsForStudentAsync(int studentId)
    {

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _context.StudentContracts
        .Where(c => c.DeletedOn == null)
        .Where(c => c.StudentId == studentId
            && c.ExpirationDate < today)
            .Include(c => c.JobInstances.Where(j => j.Status == JobInstanceStatus.Completed))
                .ThenInclude(j => j.Senior).ThenInclude(s => s.Contact)
            .ToListAsync();
    }

}