using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student> GetByIdAsync(int id);

    Task<List<Student>> GetStudentsAsync(StudentFilterDto? filter = null);

    Task<List<StudentDto>> GetStudentsWithDetailsAsync(StudentFilterDto? filter = null);
    Task<Student?> GetByStudentNumberAsync(string studentNumber);

    Task<Student> AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task GetByFacultyAsync(int facultyId);

    Task<List<Student>> FindEligibleStudentsForSchedule(
        int orderScheduleId,
        List<int>? notifiedStudentIds,
         int? preferedStudentId);
    Task<List<Student>> LoadStudentsWithIncludes(
        int? studentId,
        StudentIncludeOptions studentIncludeOptions,
         List<StudentStatus>? withStatus = null,
    List<StudentStatus>? excludeStatus = null,
    bool asNoTracking = true);
    Task<int> CountAsync(Expression<Func<Student, bool>> predicate);
    Task<List<Student>> FindEligibleStudentsForInstance(
                            DateOnly scheduledDate,
                            TimeOnly startTime,
                            TimeOnly endTime,
                            int seniorCityId,
                            List<int> serviceIds,
                            List<int> notifiedStudentIds,
                            int? preferedStudentId);

    Task<List<Student>> FindEligibleStudentsForInstance2(
   DateOnly date,
   TimeOnly startTime,
   TimeOnly endTime,
   int orderId,
int? preferedStudentId,
 List<int> excludeJobInstanceIds
   );
}