namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class CustomerRepository : ICustomerRepository
{
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context) => _context = context;

        public async Task<Customer> GetByIdAsync(int id)
            => await _context.Customers
                .Include(c => c.Contact)
                .Include(c => c.Seniors)
                .FirstOrDefaultAsync(c => c.Id == id);

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
}