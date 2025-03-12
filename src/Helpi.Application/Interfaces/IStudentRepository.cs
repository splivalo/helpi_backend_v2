using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student> GetByIdAsync(int id);

    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task<Student?> GetByStudentNumberAsync(string studentNumber);
    Task<IEnumerable<Student>> GetByVerificationStatusAsync(VerificationStatus status);
    Task<Student> AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task GetByFacultyAsync(int facultyId);

}