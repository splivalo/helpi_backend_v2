namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ContactInfoRepository : IContactInfoRepository
{
        private readonly AppDbContext _context;

        public ContactInfoRepository(AppDbContext context) => _context = context;

        public async Task<ContactInfo> GetByIdAsync(int id) => await _context.ContactInfos.FindAsync(id);
        public async Task<IEnumerable<ContactInfo>> SearchByNameAsync(string lastName, string firstName)
            => await _context.ContactInfos
                .Where(c => c.LastName.Contains(lastName) && c.FirstName.Contains(firstName))
                .ToListAsync();
        public async Task<ContactInfo> AddAsync(ContactInfo contactInfo)
        {
                await _context.ContactInfos.AddAsync(contactInfo);
                await _context.SaveChangesAsync();
                return contactInfo;
        }
        public async Task UpdateAsync(ContactInfo contactInfo)
        {
                _context.ContactInfos.Update(contactInfo);
                await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(ContactInfo contactInfo)
        {
                _context.ContactInfos.Remove(contactInfo);
                await _context.SaveChangesAsync();
        }
}