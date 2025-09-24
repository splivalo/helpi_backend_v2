using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Persistence.Extentions;

public class SeniorQueryBuilder
{
    private IQueryable<Senior> _query;

    public SeniorQueryBuilder(AppDbContext context)
    {
        _query = context.Seniors
            .Include(s => s.Contact)
            .AsNoTracking();
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
                Country = s.Contact.Country,
                PostalCode = s.Contact.PostalCode,
                CreatedAt = s.Contact.CreatedAt,
            },
            Relationship = s.Relationship,
            SpecialRequirements = s.SpecialRequirements,

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