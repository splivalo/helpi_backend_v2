namespace Helpi.Infrastructure.Repositories;

using System.Collections.Generic;
using System.Linq.Expressions;
using Helpi.Application.Common.Extensions;
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
        var student = await _context.Students.Where(s => s.DeletedAt == null).Include(s => s.Contact).SingleOrDefaultAsync(s => s.UserId == id);

        if (student == null)
        {
            throw new NotFoundException(nameof(Student), id);

        }

        return student;


    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await _context.Students
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Contact)
            .SingleOrDefaultAsync(s => s.UserId == userId);
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
                   .FilterByNoScheduleConflicts(filter.AvailabilityCriteria, filter.ExcludeConflicts)
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
            startDate: orderSchedule.Order.StartDate,
            endDate: orderSchedule.Order.EndDate,
            isoTargetDay: orderSchedule.DayOfWeek,
            targetStart: orderSchedule.StartTime,
            targetEnd: orderSchedule.EndTime,
            seniorId: orderSchedule.Order.Senior.Id,
            requiredServiceIds: requiredServiceIds,
            notifiedStudentIds: notifiedStudentIds,
            preferedStudentId: preferedStudentId,
            excludeJobInstanceIds: []
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
            startDate: scheduledDate,
            endDate: scheduledDate,
            isoTargetDay: targetDay,
            targetStart: startTime,
            targetEnd: endTime,
            seniorId: seniorId,
            requiredServiceIds: serviceIds,
            notifiedStudentIds: notifiedStudentIds,
            preferedStudentId: preferedStudentId,
            excludeJobInstanceIds: []
        );
    }

    public async Task<List<Student>> FindEligibleStudentsForInstance2(
       DateOnly scheduledDate,
       TimeOnly startTime,
       TimeOnly endTime,
       int orderId,
        int? preferedStudentId,
        List<int> excludeJobInstanceIds
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
            startDate: scheduledDate,
            endDate: scheduledDate,
            isoTargetDay: targetDay,
            targetStart: startTime,
            targetEnd: endTime,
            seniorId: order!.Senior.Id,
            requiredServiceIds: serviceIds,
            notifiedStudentIds: null,
            preferedStudentId: preferedStudentId,
            excludeJobInstanceIds: excludeJobInstanceIds ?? new List<int> { }
        );
    }

    private async Task<List<Student>> FindEligibleStudentsCore(
    DateOnly startDate,
    DateOnly endDate,
    byte isoTargetDay,
    TimeOnly targetStart,
    TimeOnly targetEnd,
    List<int> requiredServiceIds,
    List<int>? notifiedStudentIds,
    int? preferedStudentId,
    int? seniorId,
    List<int> excludeJobInstanceIds
)
    {
        bool isDateRange = startDate != endDate;

        // Travel buffer from PricingConfiguration (DB settings)
        var pricingCfg = await _context.PricingConfigurations.FirstOrDefaultAsync();
        var travelBufferMinutes = pricingCfg?.TravelBufferMinutes ?? 15;
        var bufferedStart = targetStart.AddMinutes(-travelBufferMinutes);
        var bufferedEnd = targetEnd.AddMinutes(travelBufferMinutes);


        var query = _context.Students
            .WhereIsActive()
            .Include(s => s.Contact)
            .Include(s => s.StudentServices)
            .Include(s => s.AvailabilitySlots)
            .Include(s => s.Faculty)
            .AsNoTracking()
            .AsQueryable();

        if (notifiedStudentIds?.Any() == true)
        {
            query = query.Where(s => !notifiedStudentIds.Contains(s.UserId));
        }

        // Full-containment filter: only return students whose availability slot
        // fully covers the requested time window on the target day.
        query = query.Where(s =>
            s.AvailabilitySlots.Any(a =>
                a.DayOfWeek == isoTargetDay &&
                a.StartTime <= targetStart &&
                a.EndTime >= targetEnd
            )
        );

        // v2: students no longer choose services in their app,
        // so service filter is removed from matching logic.
        // Services (6 categories) are informational for senior only.

        // ✅ Conflict detection 1: Check generated JobInstances with travel buffer
        // Only consider assignments on active orders (not Completed/Cancelled)
        query = query.Where(s =>
            !s.ScheduleAssignments
                .Where(sa => sa.Status == AssignmentStatus.Accepted
                    && sa.OrderSchedule.Order.Status != OrderStatus.Completed
                    && sa.OrderSchedule.Order.Status != OrderStatus.Cancelled)
                .SelectMany(sa => sa.JobInstances)
                .Any(j =>
                    (
                        isDateRange
                            ? (j.ScheduledDate >= startDate &&
                              j.ScheduledDate <= endDate &&
                              j.ScheduledDate.DayOfWeek == DayOfWeekExtensions.FromIsoWeekday(isoTargetDay))
                            : j.ScheduledDate == startDate
                    )
                    && j.StartTime < bufferedEnd
                    && j.EndTime > bufferedStart
                    && !excludeJobInstanceIds.Contains(j.Id)
                    && j.Status != JobInstanceStatus.Completed
                    && j.Status != JobInstanceStatus.Cancelled
                    && j.Status != JobInstanceStatus.Rescheduled
                )
        );

        // ✅ Conflict detection 2: Check recurring OrderSchedule patterns
        // Catches conflicts even when JobInstances haven't been generated yet
        // Only consider assignments whose parent Order is still active (Pending/FullAssigned)
        query = query.Where(s =>
            !s.ScheduleAssignments
                .Where(sa => sa.Status == AssignmentStatus.Accepted
                    && sa.OrderSchedule.Order.Status != OrderStatus.Completed
                    && sa.OrderSchedule.Order.Status != OrderStatus.Cancelled)
                .Any(sa =>
                    sa.OrderSchedule.DayOfWeek == isoTargetDay
                    && sa.OrderSchedule.StartTime < bufferedEnd
                    && sa.OrderSchedule.EndTime > bufferedStart
                )
        );

        var students = await query.ToListAsync();
        return await PrioritizeStudents(students, preferedStudentId, seniorId);
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
                    .ThenBy(s => senior != null ? CalculateDistance(s, senior) : double.MaxValue)
                    .ThenByDescending(s => s.AverageRating)
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

    public async Task<Dictionary<int, int>> GetCompletedJobCountsForSenior(int seniorId, List<int> studentIds)
    {
        if (studentIds.Count == 0) return new Dictionary<int, int>();

        return await _context.ScheduleAssignments
            .Where(sa => studentIds.Contains(sa.StudentId)
                && sa.OrderSchedule.Order.SeniorId == seniorId
                && sa.Status == AssignmentStatus.Accepted)
            .SelectMany(sa => sa.JobInstances.Where(j => j.Status == JobInstanceStatus.Completed))
            .GroupBy(j => j.ScheduleAssignment!.StudentId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<int> GetSeniorIdForOrderSchedule(int orderScheduleId)
    {
        return await _context.OrderSchedules
            .Where(os => os.Id == orderScheduleId)
            .Select(os => os.Order.SeniorId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetSeniorIdForOrder(int orderId)
    {
        return await _context.Orders
            .Where(o => o.Id == orderId)
            .Select(o => o.SeniorId)
            .FirstOrDefaultAsync();
    }


    public async Task<List<Student>> LoadStudentsWithIncludes(int? studentId, StudentIncludeOptions includes, List<StudentStatus>? withStatus = null,
    List<StudentStatus>? excludeStatus = null,
    bool asNoTracking = true)
    {
        var query = _context.Students.AsQueryable();

        query = query.Where(s => s.DeletedAt == null);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

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
            query = query.Include(s => s.Contracts.Where(c => c.DeletedOn == null));
        }



        return await query.ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<Student, bool>> predicate)
    {
        return _context.Students.Where(s => s.DeletedAt == null).CountAsync(predicate);

    }


    public static byte ToIsoDayNumber(DateOnly date)
    {
        // .NET: Sunday = 0, Monday = 1, ..., Saturday = 6
        // DB:   Monday = 1, ..., Saturday = 6, Sunday = 7
        int dotNetDay = (int)date.DayOfWeek;
        return (byte)(dotNetDay == 0 ? 7 : dotNetDay);
    }


}

