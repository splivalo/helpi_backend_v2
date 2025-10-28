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
      List<int>? notifiedStudentIds = null,
      int? preferedStudentId = null)
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
            seniorId: orderSchedule.Order.Senior.Id,
            requiredServiceIds: requiredServiceIds,
            notifiedStudentIds: notifiedStudentIds,
            preferedStudentId: preferedStudentId
        );
    }

    public async Task<List<Student>> FindEligibleStudentsForInstance(
        DateOnly scheduledDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int seniorId,
        List<int> serviceIds,
        List<int>? notifiedStudentIds = null,
        int? preferedStudentId = null)
    {
        var targetDay = ToIsoDayNumber(scheduledDate);

        return await FindEligibleStudentsCore(
            scheduledDate: scheduledDate,
            targetDay: targetDay,
            targetStart: startTime,
            targetEnd: endTime,
            seniorId: seniorId,
            requiredServiceIds: serviceIds,
            notifiedStudentIds: notifiedStudentIds,
            preferedStudentId: preferedStudentId
        );
    }

    public async Task<List<Student>> FindEligibleStudentsForInstance2(
       DateOnly scheduledDate,
       TimeOnly startTime,
       TimeOnly endTime,
       int orderId,
int? preferedStudentId
       )
    {

        var order = await _context.Orders.AsNoTracking()
                    .Where(o => o.Id == orderId)
                    .Include(o => o.Senior).ThenInclude(s => s.Contact)
                    .Include(o => o.OrderServices)
                    .FirstOrDefaultAsync();

        if (order == null)
        {
            return [];
        }

        var serviceIds = order.OrderServices.Select(os => os.ServiceId).ToList();


        var targetDay = ToIsoDayNumber(scheduledDate);


        return await FindEligibleStudentsCore(
            scheduledDate: scheduledDate,
            targetDay: targetDay,
            targetStart: startTime,
            targetEnd: endTime,
            seniorId: order!.Senior.Id,
            requiredServiceIds: serviceIds,
            notifiedStudentIds: null,
            preferedStudentId: preferedStudentId
        );
    }

    private async Task<List<Student>> FindEligibleStudentsCore(
        DateOnly? scheduledDate,
        byte targetDay,
        TimeOnly targetStart,
        TimeOnly targetEnd,
        List<int> requiredServiceIds,
        List<int>? notifiedStudentIds,
       int? preferedStudentId,
        int? seniorId
        )
    {
        bool matchFullSchedule = scheduledDate == null;

        var query = _context.Students.WhereIsActive()
            .Include(s => s.Contact)
            // .Include(s => s.StudentServices)
            // .Include(s => s.AvailabilitySlots)
            // .Include(s => s.ScheduleAssignments)
            //     .ThenInclude(sa => sa.OrderSchedule)
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

        if (matchFullSchedule)
        {
            // schedule Conflict  check
            query = query.Where(s => !s.ScheduleAssignments
                        .SelectMany(sa => sa.JobInstances)
                        .Any(j =>
                            j.ScheduledDate == scheduledDate &&
                            j.StartTime < targetEnd &&
                            j.EndTime > targetStart &&
                            j.Status != JobInstanceStatus.Completed &&
                            j.Status != JobInstanceStatus.Cancelled &&
                            j.Status != JobInstanceStatus.Rescheduled
                        ));

        }
        else
        {
            // Day Conflict  check  
            query = query.Include(s => s.ScheduleAssignments.Where(sa => sa.Status == AssignmentStatus.Accepted))
                            .ThenInclude(sa => sa.JobInstances)
                         .Where(s => !s.ScheduleAssignments.Any(sa =>
                            sa.JobInstances.Any(j =>
                                j.ScheduledDate == scheduledDate &&
                                j.StartTime < targetEnd &&
                                j.EndTime > targetStart &&
                                j.Status != JobInstanceStatus.Completed &&
                                j.Status != JobInstanceStatus.Cancelled &&
                                j.Status != JobInstanceStatus.Rescheduled
                            )
                        ));
        }

        var students = await query.ToListAsync();
        students = await PrioritizeStudents(students, preferedStudentId, seniorId);
        return students;
    }

    private async Task<List<Student>> PrioritizeStudents(
        List<Student> students,
        int? preferedStudentId,
        int? seniorId)
    {

        var senior = await _context.Seniors
                            .Include(s => s.Contact)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == seniorId);


        // 1. Find students who've worked with this senior before
        // var previouslyWorkedWith = await _studentRepository.StudentsWhoWorkedWithSenior(order.SeniorId);
        // var prioritized = students
        //     .OrderByDescending(s => previouslyWorkedWith.Contains(s.Id)) 
        //     .ThenByDescending(s => s.AverageRating)                     
        //     .ThenBy(s => senior != null ? CalculateDistance(s, senior) : double.MaxValue) 
        //     .ToList();

        var prioritized = students
                    .OrderByDescending(s => preferedStudentId.HasValue && s.UserId == preferedStudentId.Value)
                    .OrderByDescending(s => s.AverageRating)
                    .ThenBy(s => senior != null ? CalculateDistance(s, senior) : double.MaxValue)
                    .ToList();

        return prioritized;

    }

    private double CalculateDistance(Student student, Senior senior)
    {


        if (student?.Contact == null || senior?.Contact == null)
            return 0;

        var lat1 = (double)student.Contact.Latitude;
        var lon1 = (double)student.Contact.Longitude;
        var lat2 = (double)senior.Contact.Latitude;
        var lon2 = (double)senior.Contact.Longitude;


        const double R = 6371; // Earth radius in kilometers
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double DegreesToRadians(double deg) => deg * (Math.PI / 180);



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


    public static byte ToIsoDayNumber(DateOnly date)
    {
        // .NET: Sunday = 0, Monday = 1, ..., Saturday = 6
        // DB:   Monday = 1, ..., Saturday = 6, Sunday = 7
        int dotNetDay = (int)date.DayOfWeek;
        return (byte)(dotNetDay == 0 ? 7 : dotNetDay);
    }


}

