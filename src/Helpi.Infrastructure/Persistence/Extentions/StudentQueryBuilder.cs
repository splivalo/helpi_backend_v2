using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Persistence.Extentions;

public class StudentQueryBuilder
{
    private IQueryable<Student> _query;
    private readonly AppDbContext _context;

    public StudentQueryBuilder(AppDbContext context)
    {
        _context = context;
        _query = context.Students
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Contact)
            .Include(s => s.Faculty)
            .AsNoTracking();
    }

    public StudentQueryBuilder FilterByCity(int? cityId)
    {
        if (cityId.HasValue)
        {
            _query = _query.Where(s => s.Contact.CityId == cityId.Value);
        }
        return this;
    }

    public StudentQueryBuilder FilterBySearchText(string? searchText)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var searchTerm = searchText.Trim().ToLower();
            _query = _query.Where(s =>
                s.Contact.FullName.ToLower().Contains(searchTerm) ||
                s.StudentNumber.ToLower().Contains(searchTerm));
        }
        return this;
    }

    public StudentQueryBuilder FilterByServices(List<int>? serviceIds)
    {
        if (serviceIds != null && serviceIds.Any())
        {
            _query = _query.Where(s =>
     serviceIds.All(requiredId =>
         s.StudentServices.Any(ss => ss.ServiceId == requiredId)));
        }
        return this;
    }

    public StudentQueryBuilder FilterByStatus(StudentStatus? status)
    {
        if (status.HasValue)
        {
            _query = _query.Where(s => s.Status == status.Value);
        }
        return this;
    }

    public StudentQueryBuilder FilterByFaculty(int? facultyId)
    {
        if (facultyId.HasValue)
        {
            _query = _query.Where(s => s.FacultyId == facultyId.Value);
        }
        return this;
    }

    public StudentQueryBuilder FilterByAvailability(List<AvailabilityCriteria>? availabilityCriteria, bool matchAll = false)
    {
        if (availabilityCriteria != null && availabilityCriteria.Any())
        {
            if (matchAll)
            {
                // Student must match ALL availability criteria (AND logic)
                foreach (var criteria in availabilityCriteria)
                {
                    var isoDayByte = criteria.DayOfWeek;

                    if (criteria.StartTime.HasValue && criteria.EndTime.HasValue)
                    {
                        // Must be available during the specified time range on this specific day
                        _query = _query.Where(s => s.AvailabilitySlots.Any(slot =>
                            slot.DayOfWeek == isoDayByte &&
                            slot.StartTime <= criteria.StartTime.Value &&
                            slot.EndTime >= criteria.EndTime.Value));
                    }
                    else
                    {
                        // Must be available on this specific day (any time)
                        _query = _query.Where(s => s.AvailabilitySlots.Any(slot => slot.DayOfWeek == isoDayByte));
                    }
                }
            }
            else
            {
                // Student must match ANY availability criteria (OR logic)
                var predicates = new List<Expression<Func<Student, bool>>>();

                foreach (var criteria in availabilityCriteria)
                {
                    var dayByte = (byte)criteria.DayOfWeek;

                    if (criteria.StartTime.HasValue && criteria.EndTime.HasValue)
                    {
                        var startTime = criteria.StartTime.Value;
                        var endTime = criteria.EndTime.Value;
                        predicates.Add(s => s.AvailabilitySlots.Any(slot =>
                            slot.DayOfWeek == dayByte &&
                            slot.StartTime <= startTime &&
                            slot.EndTime >= endTime));
                    }
                    else
                    {
                        predicates.Add(s => s.AvailabilitySlots.Any(slot => slot.DayOfWeek == dayByte));
                    }
                }

                // Combine all predicates with OR logic
                if (predicates.Any())
                {
                    var combinedPredicate = predicates.Aggregate((left, right) =>
                        Expression.Lambda<Func<Student, bool>>(
                            Expression.OrElse(left.Body, right.Body), left.Parameters));

                    _query = _query.Where(combinedPredicate);
                }
            }
        }
        return this;
    }

    public StudentQueryBuilder FilterByHasAvailabilitySlots(bool? hasSlots)
    {
        if (hasSlots.HasValue)
        {
            if (hasSlots.Value)
            {
                _query = _query.Where(s => s.AvailabilitySlots.Any());
            }
            else
            {
                _query = _query.Where(s => !s.AvailabilitySlots.Any());
            }
        }
        return this;
    }

    public StudentQueryBuilder FilterByMinRating(decimal? minRating)
    {
        if (minRating.HasValue)
        {
            _query = _query.Where(s => s.AverageRating >= minRating.Value);
        }
        return this;
    }

    public StudentQueryBuilder FilterByBackgroundCheck(bool? backgroundCheckCompleted)
    {
        if (backgroundCheckCompleted.HasValue)
        {
            if (backgroundCheckCompleted.Value)
            {
                _query = _query.Where(s => s.BackgroundCheckDate.HasValue);
            }
            else
            {
                _query = _query.Where(s => !s.BackgroundCheckDate.HasValue);
            }
        }
        return this;
    }

    public StudentQueryBuilder IncludeDeleted(bool includeDeleted = false)
    {
        if (!includeDeleted)
        {
            _query = _query.Where(s => s.DeletedAt == null);
        }
        return this;
    }

    public StudentQueryBuilder OrderByName()
    {
        _query = _query.OrderBy(s => s.Contact.FullName);
        return this;
    }

    public StudentQueryBuilder OrderByRating()
    {
        _query = _query.OrderByDescending(s => s.AverageRating);
        return this;
    }

    public StudentQueryBuilder OrderByRegistrationDate()
    {
        _query = _query.OrderByDescending(s => s.DateRegistered);
        return this;
    }

    // Method to get students with complete information via projection
    public async Task<List<StudentDto>> ExecuteWithDetailsAsync()
    {
        return await _query.Select(s => new StudentDto
        {
            UserId = s.UserId,
            StudentNumber = s.StudentNumber,
            FacultyId = s.FacultyId,
            DateRegistered = s.DateRegistered,
            Status = s.Status,
            DeletedAt = s.DeletedAt,
            BackgroundCheckDate = s.BackgroundCheckDate,
            AverageRating = s.AverageRating,

            Contact = new ContactInfoDto
            {
                Id = s.Contact.Id,
                FullName = s.Contact.FullName,
                Gender = s.Contact.Gender,
                Phone = s.Contact.Phone,
                Email = s.Contact.Email,
                FullAddress = s.Contact.FullAddress,
                GooglePlaceId = s.Contact.GooglePlaceId,
                CityId = s.Contact.CityId,
                CityName = s.Contact.CityName,
                Country = s.Contact.Country,
                PostalCode = s.Contact.PostalCode,
                CreatedAt = s.Contact.CreatedAt,
            },

            Faculty = new FacultyDto
            {
                Id = s.Faculty.Id,
                Translations = s.Faculty.Translations
            },
        }).ToListAsync();
    }

    // Method for basic student entities 
    public async Task<List<Student>> ExecuteAsync()
    {
        return await _query.ToListAsync();
    }


}
