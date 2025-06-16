namespace Helpi.Infrastructure.Repositories;

using System;
using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class CustomerRepository : ICustomerRepository
{
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context) => _context = context;

        public async Task<Customer?> GetByIdAsync(int id)
        {
                var customer = await _context.Customers
                  .AsNoTracking()
                  .Include(c => c.Contact)
                  .Include(c => c.Seniors)
                  .ThenInclude(s => s.Contact)
                  .IgnoreAutoIncludes()
                  .SingleOrDefaultAsync(c => c.UserId == id);


                return customer;
        }

        public async Task<Customer> GetByContactIdAsync(int contactId)
            => await _context.Customers
                .FirstOrDefaultAsync(c => c.ContactId == contactId);

        public async Task<IEnumerable<Customer>> GetCustomersByNotificationMethod(NotificationMethod method)
            => await _context.Customers
                .Where(c => c.PreferredNotificationMethod == method)
                .ToListAsync();

        public async Task<Customer> AddAsync(Customer customer)
        {
                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
                return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Customer customer)
        {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
                return await _context.Customers
                        .IgnoreAutoIncludes()
                        .Include(c => c.Contact)
                        .Include(c => c.Seniors)
                        .ThenInclude(s => s.Contact)
                        .ToListAsync();
        }

        public Task<int> CountAsync(Expression<Func<Customer, bool>> predicate)
        {
                return _context.Customers.CountAsync(predicate);
        }

        public async Task<Customer?> LoadCustomerWithIncludes(int customerId, CustomerIncludeOptions includes)
        {
                var query = _context.Customers.AsQueryable();

                if (includes.Contact)
                        query = query.Include(c => c.Contact);

                if (includes.Seniors)
                {
                        query = query.Include(c => c.Seniors);
                }




                return await query.FirstOrDefaultAsync(c => c.UserId == customerId);
        }
}

