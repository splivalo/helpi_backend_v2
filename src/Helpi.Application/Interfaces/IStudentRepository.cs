using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student> GetByIdAsync(int id);

    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task<Student?> GetByStudentNumberAsync(string studentNumber);
    Task<IEnumerable<Student>> GetByVerificationStatusAsync(StudentStatus status);
    Task<Student> AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task GetByFacultyAsync(int facultyId);

    Task<List<Student>> FindEligibleStudentsForSchedule(int orderScheduleId, List<int>? notifiedStudentIds);
    Task<List<Student>> LoadStudentsWithIncludes(int? studentId, StudentIncludeOptions studentIncludeOptions);
    Task<int> CountAsync(Expression<Func<Student, bool>> predicate);

}