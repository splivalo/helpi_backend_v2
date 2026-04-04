using System.Linq.Expressions;
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> LoadCustomerWithIncludes(int customerId, CustomerIncludeOptions includes);
    Task<Customer?> GetByContactIdAsync(int contactId);
    Task<IEnumerable<Customer>> GetCustomersByNotificationMethod(NotificationMethod method);
    Task<Customer> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<int> CountAsync(Expression<Func<Customer, bool>> predicate);

}