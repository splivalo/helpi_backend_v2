using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IStudentServiceRepository
{
    Task<StudentService?> GetByIdAsync(int studentId, int serviceId);
    Task<IEnumerable<StudentService>> GetByStudentAsync(int studentId);
    Task<IEnumerable<StudentService>> GetByServiceAsync(int serviceId);
    Task<StudentService> AddAsync(StudentService studentService);
    Task UpdateAsync(StudentService studentService);
    Task DeleteAsync(int studentId, int serviceId);
}