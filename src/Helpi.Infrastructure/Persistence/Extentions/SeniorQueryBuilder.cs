using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Persistence.Extentions;

public class SeniorQueryBuilder
{
    private IQueryable<Senior> _query;
    private readonly AppDbContext _context;

    public SeniorQueryBuilder(AppDbContext context)
    {
        _context = context;
        _query = context.Seniors
            .Include(s => s.Contact)
            .Include(s => s.Customer)
                .ThenInclude(c => c.Contact)
            .AsNoTracking();
    }

    /// <summary>
    /// Excludes archived seniors (DeletedAt != null). Use for non-admin queries.
    /// </summary>
    public SeniorQueryBuilder ExcludeArchived()
    {
        _query = _query.Where(s => s.DeletedAt == null);
        return this;
    }

    public SeniorQueryBuilder FilterByCity(int? cityId)
    {
        if (cityId.HasValue)
        {
            _query = _query.Where(s => s.Contact.CityId == cityId.Value);
        }
        return this;
    }

    public SeniorQueryBuilder FilterByOrderStatus(OrderStatus? orderStatus)
    {
        if (orderStatus.HasValue)
        {
            if (orderStatus.Value == OrderStatus.InActive)
            {
                // Seniors with no orders
                _query = _query.Where(s => !s.Orders.Any());
            }
            else
            {
                // Seniors with at least one order with the given status
                _query = _query.Where(s => s.Orders.Any(o => o.Status == orderStatus.Value));
            }
        }
        return this;
    }


    public SeniorQueryBuilder FilterBySearchText(string? searchText)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var searchTerm = searchText.Trim().ToLower();
            _query = _query.Where(s =>
                s.Contact.FullName.ToLower().Contains(searchTerm));
        }
        return this;
    }

    public SeniorQueryBuilder OrderByName()
    {
        _query = _query.OrderBy(s => s.Contact.FullName);
        return this;
    }

    // Method to get seniors with order status information via projection
    public async Task<List<SeniorDto>> ExecuteWithExtraDetailsAsync()
    {
        return await _query.Select(s => new SeniorDto
        {
            Id = s.Id,
            CustomerId = s.CustomerId,

            // From User table (Customer.UserId == Senior.CustomerId)
            IsSuspended = _context.Users
                .Where(u => u.Id == s.CustomerId)
                .Select(u => u.IsSuspended)
                .FirstOrDefault(),
            SuspensionReason = _context.Users
                .Where(u => u.Id == s.CustomerId)
                .Select(u => u.SuspensionReason)
                .FirstOrDefault(),

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
                DateOfBirth = s.Contact.DateOfBirth,
                ProfileImageUrl = s.Contact.ProfileImageUrl,
            },
            // Orderer contact - only populated when Relationship != Self
            OrdererContact = s.Relationship != Relationship.Self && s.Customer != null && s.Customer.Contact != null
                ? new ContactInfoDto
                {
                    Id = s.Customer.Contact.Id,
                    FullName = s.Customer.Contact.FullName,
                    Gender = s.Customer.Contact.Gender,
                    Phone = s.Customer.Contact.Phone,
                    Email = s.Customer.Contact.Email,
                    FullAddress = s.Customer.Contact.FullAddress,
                    GooglePlaceId = s.Customer.Contact.GooglePlaceId,
                    CityId = s.Customer.Contact.CityId,
                    CityName = s.Customer.Contact.CityName,
                    Country = s.Customer.Contact.Country,
                    PostalCode = s.Customer.Contact.PostalCode,
                    CreatedAt = s.Customer.Contact.CreatedAt,
                    DateOfBirth = s.Customer.Contact.DateOfBirth,
                    ProfileImageUrl = s.Customer.Contact.ProfileImageUrl,
                }
                : null,
            Relationship = s.Relationship,
            SpecialRequirements = s.SpecialRequirements,
            DeletedAt = s.DeletedAt,

            // Extra information
            OrderStatuses = s.Orders
                .Select(o => o.Status)
                .Distinct()
                .ToList(),
        }).ToListAsync();
    }

    public async Task<List<Senior>> ExecuteAsync()
    {
        return await _query.ToListAsync();
    }
}