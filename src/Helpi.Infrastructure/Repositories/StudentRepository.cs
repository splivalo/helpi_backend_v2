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
        var orderSchedule = await _context.OrderSchedules
            .Include(os => os.Order)
                .ThenInclude(o => o.OrderServices)
            .Include(os => os.Order)
                .ThenInclude(o => o.Senior)
                .ThenInclude(s => s.Contact)
            .FirstOrDefaultAsync(os => os.Id == orderScheduleId);

        if (orderSchedule == null) return new List<Student>();

        var requiredServiceIds = orderSchedule.Order.OrderServices
            .Select(os => os.ServiceId)
            .ToList();

        return await FindEligibleStudentsCore(
            scheduledDate: null,
            targetDay: orderSchedule.DayOfWeek,
            targetStart: orderSchedule.StartTime,
            targetEnd: orderSchedule.EndTime,
            seniorCityId: orderSchedule.Order.Senior.Contact.CityId,
            requiredServiceIds: requiredServiceIds,
            notifiedStudentIds: notifiedStudentIds
        );
    }

    public async Task<List<Student>> FindEligibleStudentsForInstance(
        DateOnly scheduledDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int seniorCityId,
        List<int> serviceIds,
        List<int>? notifiedStudentIds = null)
    {
        var targetDay = (byte)scheduledDate.DayOfWeek;

        return await FindEligibleStudentsCore(
            scheduledDate: scheduledDate,
            targetDay: targetDay,
            targetStart: startTime,
            targetEnd: endTime,
            seniorCityId: seniorCityId,
            requiredServiceIds: serviceIds,
            notifiedStudentIds: notifiedStudentIds
        );
    }

    private async Task<List<Student>> FindEligibleStudentsCore(
        DateOnly? scheduledDate,
        byte targetDay,
        TimeOnly targetStart,
        TimeOnly targetEnd,
        int seniorCityId,
        List<int> requiredServiceIds,
        List<int>? notifiedStudentIds)
    {
        var query = _context.Students.WhereIsActive()
            .Include(s => s.Contact)
            .Include(s => s.StudentServices)
            .Include(s => s.AvailabilitySlots)
            .Include(s => s.ScheduleAssignments)
                .ThenInclude(sa => sa.OrderSchedule)
            .AsNoTracking()
            .AsQueryable();

        // Filter out already notified students
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

        // Student must offer ALL required services
        query = query.Where(s => requiredServiceIds.All(rs =>
            s.StudentServices.Any(ss => ss.ServiceId == rs)
        ));

        // Location validation (optional, uncomment if needed)
        // query = query.Where(s =>
        //     s.Contact.CityId == seniorCityId &&
        //     _context.ServiceRegions.Any(sr =>
        //         sr.CityId == seniorCityId &&
        //         sr.active &&
        //         requiredServiceIds.Contains(sr.ServiceId)
        // ));

        // Conflict check
        query = query.Where(s => !s.ScheduleAssignments.Any(sa =>
            sa.OrderSchedule.DayOfWeek == targetDay &&
            sa.OrderSchedule.StartTime < targetEnd &&
            sa.OrderSchedule.EndTime > targetStart &&
            sa.Status != AssignmentStatus.Completed &&
            sa.Status != AssignmentStatus.Terminated &&
            sa.Status != AssignmentStatus.Declined
        ));

        return await query.ToListAsync();
    }


    public async Task<List<Student>> LoadStudentsWithIncludes(int? studentId, StudentIncludeOptions includes, List<StudentStatus>? withStatus = null,
    List<StudentStatus>? excludeStatus = null)
    {
        var query = _context.Students.AsQueryable();

        // Status filter
        if (withStatus != null && withStatus.Any())
        {
            query = query.Where(s => withStatus.Contains(s.Status));
        }


        if (excludeStatus != null && excludeStatus.Any())
        {
            query = query.Where(s => !excludeStatus.Contains(s.Status));
        }

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