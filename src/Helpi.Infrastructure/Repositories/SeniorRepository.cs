namespace Helpi.Infrastructure.Repositories;

using System.Collections.Generic;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Helpi.Infrastructure.Persistence.Extentions;
using Microsoft.EntityFrameworkCore;

public class SeniorRepository : ISeniorRepository
{
        private readonly AppDbContext _context;

        public SeniorRepository(AppDbContext context) => _context = context;

        public async Task<Senior?> GetByIdAsync(int id)
        {
                return await _context.Seniors
                 .Include(s => s.Contact).SingleOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Senior>> GetByCustomerIdAsync(int customerId)
            => await _context.Seniors
                .Where(s => s.CustomerId == customerId)
                .ToListAsync();

        public async Task<IEnumerable<Senior>> GetByRelationshipAsync(Relationship relationship)
            => await _context.Seniors
                .Where(s => s.Relationship == relationship)
                .ToListAsync();

        public async Task<Senior> AddAsync(Senior senior)
        {
                await _context.Seniors.AddAsync(senior);
                await _context.SaveChangesAsync();
                return senior;
        }

        public async Task UpdateAsync(Senior senior)
        {
                _context.Seniors.Update(senior);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Senior senior)
        {
                _context.Seniors.Remove(senior);
                await _context.SaveChangesAsync();
        }

        public async Task<List<Senior>> GetSeniorsAsync()
        {
                return await _context.Seniors
                .Include(s => s.Contact)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Senior>> GetSeniorsAsync(SeniorFilterDto? filter = null)
        {
                var builder = new SeniorQueryBuilder(_context);

                if (filter != null)
                {
                        builder.FilterByCity(filter.CityId)
                               .FilterByOrderStatus(filter.OrderStatus)
                               .FilterBySearchText(filter.SearchText);
                }

                return await builder.OrderByName().ExecuteAsync();
        }

        public async Task<List<SeniorDto>> GetSeniorsWithExtraDetailsAsync(SeniorFilterDto? filter = null)
        {
                var builder = new SeniorQueryBuilder(_context);

                if (filter != null)
                {
                        builder.FilterByCity(filter.CityId)
                               .FilterByOrderStatus(filter.OrderStatus)
                               .FilterBySearchText(filter.SearchText);
                }

                return await builder.OrderByName().ExecuteWithExtraDetailsAsync();
        }
}