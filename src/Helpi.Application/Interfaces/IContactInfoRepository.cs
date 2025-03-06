using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IContactInfoRepository
{
    Task<ContactInfo> GetByIdAsync(int id);
    Task<IEnumerable<ContactInfo>> SearchByNameAsync(string lastName, string firstName);
    Task<ContactInfo> AddAsync(ContactInfo contactInfo);
    Task UpdateAsync(ContactInfo contactInfo);
    Task DeleteAsync(ContactInfo contactInfo);
}