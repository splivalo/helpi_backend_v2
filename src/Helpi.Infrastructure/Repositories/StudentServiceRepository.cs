namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


public class StudentServiceRepository : IStudentServiceRepository
{
        private readonly AppDbContext _context;

        public StudentServiceRepository(AppDbContext context) => _context = context;

        public async Task<StudentService?> GetByIdAsync(int studentId, int serviceId)
        {
                return await _context.StudentServices
                .SingleOrDefaultAsync(ss => ss.StudentId == studentId && ss.ServiceId == serviceId);
        }

        public async Task<IEnumerable<StudentService>> GetByStudentAsync(int studentId)
            => await _context.StudentServices.Where(ss => ss.StudentId == studentId).ToListAsync();

        public async Task<IEnumerable<StudentService>> GetByServiceAsync(int serviceId)
            => await _context.StudentServices.Where(ss => ss.ServiceId == serviceId).ToListAsync();

        public async Task<StudentService> AddAsync(StudentService studentService)
        {
                await _context.StudentServices.AddAsync(studentService);
                await _context.SaveChangesAsync();
                return studentService;
        }

        public async Task<List<StudentService>> AddRangeAsync(List<StudentService> studentServices)
        {
                await _context.StudentServices.AddRangeAsync(studentServices);
                await _context.SaveChangesAsync();
                return studentServices;
        }


        public async Task UpdateAsync(StudentService studentService)
        {
                _context.StudentServices.Update(studentService);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int studentId, int serviceId)
        {
                var entity = await GetByIdAsync(studentId, serviceId);
                if (entity != null)
                {
                        _context.StudentServices.Remove(entity);
                        await _context.SaveChangesAsync();
                }
        }

        public async Task DeleteRangeAsync(int studentId, List<int> serviceIds)
        {
                var entities = await _context.StudentServices
                    .Where(s => s.StudentId == studentId && serviceIds.Contains(s.ServiceId))
                    .ToListAsync();

                if (entities.Any())
                {
                        _context.StudentServices.RemoveRange(entities);
                        await _context.SaveChangesAsync();
                }
        }


}