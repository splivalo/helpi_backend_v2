using System.Linq.Expressions;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IStudentContractRepository
{
    Task<StudentContract> GetByIdAsync(int id);
    Task<IEnumerable<StudentContract>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<StudentContract>> GetActiveContracts(DateOnly date);
    Task<StudentContract> AddAsync(StudentContract contract);
    Task UpdateAsync(StudentContract contract);
    Task DeleteAsync(StudentContract contract);

    Task<int> CountAsync(Expression<Func<StudentContract, bool>> predicate);
    Task<List<StudentContract>> GetCompletedContractsForStudentAsync(int studentId);

}