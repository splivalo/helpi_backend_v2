namespace Helpi.Infrastructure.Repositories;

using System.Collections.Generic;
using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Persistence.Extentions;
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

    public async Task<IEnumerable<Student>> GetByVerificationStatusAsync(StudentStatus status)
        => await _context.Students.Where(s => s.Status == status).ToListAsync();

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
        // _context.Students.Remove(student);
        // await _context.SaveChangesAsync();
    }

    public Task GetByFacultyAsync(int facultyId)
    {
        throw new NotImplementedException();
    }


    public async Task<List<Student>> GetStudentsAsync(StudentFilterDto? filter = null)
    {
        var builder = new StudentQueryBuilder(_context);

        return await builder.OrderByName().ExecuteAsync();
    }

    public async Task<List<StudentDto>> GetStudentsWithDetailsAsync(StudentFilterDto? filter = null)
    {
        var builder = new StudentQueryBuilder(_context);

        if (filter != null)
        {
            builder.FilterByCity(filter.CityId)
                   .FilterBySearchText(filter.SearchText)
                   .FilterByServices(filter.ServiceIds)
                   .FilterByStatus(filter.Status)
                   .FilterByFaculty(filter.FacultyId)
                   .FilterByAvailability(filter.AvailabilityCriteria, filter.MatchAllAvailability)
                   .FilterByMinRating(filter.MinAverageRating)
                   .FilterByBackgroundCheck(filter.BackgroundCheckCompleted)
                   .IncludeDeleted(filter.IncludeDeleted ?? false);
        }

        return await builder.OrderByName().ExecuteWithDetailsAsync();
    }


    public async Task<List<Student>> FindEligibleStudentsForSchedule(
        int orderScheduleId,
        List<int>? notifiedStudentIds = null)
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

        // Base query for eligible students
        var query = _context.Students.WhereIsActive()
            .Include(s => s.Contact)
            .Include(s => s.StudentServices)
            .Include(s => s.AvailabilitySlots)
            .Include(s => s.ScheduleAssignments)
                .ThenInclude(sa => sa.OrderSchedule)
            .AsNoTracking()
            .AsQueryable();


        // Filter out already notified students if a list is provided
        if (notifiedStudentIds != null && notifiedStudentIds.Any())
        {
            query = query.Where(s => !notifiedStudentIds.Contains(s.UserId));
        }

        // Availability check 
        query = query.Where(s => s.AvailabilitySlots.Any(a =>
            a.DayOfWeek == targetDay &&
            a.StartTime <= targetStart &&
            a.EndTime >= targetEnd
        ));

        // Student must offer ALL services required by the order
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
            sa.Status != AssignmentStatus.Canceled &&
            sa.Status != AssignmentStatus.Declined
        ));



        return await query.ToListAsync();
    }

    public async Task<List<Student>> LoadStudentsWithIncludes(int? studentId, StudentIncludeOptions includes)
    {
        var query = _context.Students.AsQueryable().WhereIsActive();

        if (studentId != null)
        {
            query = query.Where(s => s.UserId == studentId);
        }

        if (includes.ContactInfo)
            query = query.Include(s => s.Contact);


        if (includes.Contracts)
        {
            query = query.Include(s => s.Contracts);
        }



        return await query.ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<Student, bool>> predicate)
    {
        return _context.Students.CountAsync(predicate);
    }



}