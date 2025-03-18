namespace Helpi.Infrastructure.Repositories;

using System.Collections.Generic;
using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StudentRepository : IStudentRepository
{
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context) => _context = context;



        public async Task<Student> GetByIdAsync(int id)
        {
                var student = await _context.Students.Include(s => s.Contact).SingleOrDefaultAsync(s => s.UserId == id);

                if (student == null)
                {
                        throw new NotFoundException(nameof(Student), id);

                }

                return student;


        }

        public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
        {
                return await _context.Students.SingleOrDefaultAsync(s => s.StudentNumber == studentNumber);

        }

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

        public Task GetByFacultyAsync(int facultyId)
        {
                throw new NotImplementedException();
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
                return await _context.Students.Include(s => s.Contact).ToListAsync();
        }

        public async Task<List<Student>> UnnotifiedStudentsOfferingServices(
                List<int> serviceIds,
                List<int> notifiedStudentIds)
        {
                var students = await _context.Students
                        .Where(s => !notifiedStudentIds.Contains(s.UserId))
                        .Where(s => s.StudentServices
                                .Select(ss => ss.ServiceId)
                                .Distinct()
                                .Count(serviceId => serviceIds.Contains(serviceId)) == serviceIds.Count)
                        .ToListAsync();

                return students;
        }
}