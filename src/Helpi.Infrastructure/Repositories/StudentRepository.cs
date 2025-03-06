namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StudentRepository : IStudentRepository
{
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context) => _context = context;

        public async Task<Student> GetByIdAsync(int id) => await _context.Students
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<Student> GetByStudentNumberAsync(string studentNumber)
            => await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

        public async Task<IEnumerable<Student>> GetByVerificationStatusAsync(VerificationStatus status)
            => await _context.Students.Where(s => s.VerificationStatus == status).ToListAsync();

        public async Task<Student> AddAsync(Student student)
        {
                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();
                return student;
        }

        public async Task UpdateAsync(Student student)
        {
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Student student)
        {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
        }


}