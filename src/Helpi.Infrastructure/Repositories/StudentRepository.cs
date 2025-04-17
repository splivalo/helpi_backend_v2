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

        public async Task<List<Student>> GetAvailableStudentsForOrderSchedule(int orderScheduleId)
        {
                // Fetch the target OrderSchedule with required relationships
                var orderSchedule = await _context.OrderSchedules
                    .Include(os => os.Order)
                        .ThenInclude(o => o.OrderServices)
                    .Include(os => os.Order)
                        .ThenInclude(o => o.Senior)
                        .ThenInclude(s => s.Contact)
                    .FirstOrDefaultAsync(os => os.Id == orderScheduleId);

                if (orderSchedule == null) return new List<Student>();

                // Extract required parameters
                var requiredServiceIds = orderSchedule.Order.OrderServices
                    .Select(os => os.ServiceId)
                    .ToList();

                var seniorCityId = orderSchedule.Order.Senior.Contact.CityId;
                var targetDay = orderSchedule.DayOfWeek;
                var targetStart = orderSchedule.StartTime;
                var targetEnd = orderSchedule.EndTime;

                // Base query for available students
                var query = _context.Students
                    .Where(s => s.VerificationStatus == VerificationStatus.Verified)
                    .Include(s => s.Contact)
                    .Include(s => s.StudentServices)
                    .Include(s => s.AvailabilitySlots)
                    .Include(s => s.ScheduleAssignments)
                        .ThenInclude(sa => sa.OrderSchedule)
                    .AsQueryable();

                // Availability check 
                query = query.Where(s => s.AvailabilitySlots.Any(a =>
                    a.DayOfWeek == targetDay &&
                    a.StartTime <= targetStart &&
                    a.EndTime >= targetEnd
                ));

                // Student must offer all service required by the order
                query = query.Where(s => requiredServiceIds.All(rs =>
                    s.StudentServices.Any(ss => ss.ServiceId == rs)
                ));

                // Location validation
                // query = query.Where(s =>
                //     s.Contact.CityId == seniorCityId &&
                //     _context.ServiceRegions.Any(sr =>
                //         sr.CityId == seniorCityId &&
                //         sr.active &&
                //         requiredServiceIds.Contains(sr.ServiceId)
                // ));

                // Conflict check with existing assignments
                query = query.Where(s => !s.ScheduleAssignments.Any(sa =>
                    sa.OrderSchedule.DayOfWeek == targetDay &&
                    sa.OrderSchedule.StartTime < targetEnd &&
                    sa.OrderSchedule.EndTime > targetStart &&
                    sa.Status != AssignmentStatus.Completed &&
                    sa.Status != AssignmentStatus.Canceled
                ));

                return await query.ToListAsync();
        }

}