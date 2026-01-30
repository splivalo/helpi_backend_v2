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
        public async Task<IEnumerable<ContactInfo>> SearchByFullNameAsync(string fullName)
            => await _context.ContactInfos
                .Where(c => c.FullName.Contains(fullName))
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

        public async Task AnonymizeContactAsync(ContactInfo contact)
        {
                contact.Phone = "";
                contact.Email = $"deleted_{contact.Id}@deleted.local";
                contact.FullAddress = "";
                contact.PostalCode = "";
                contact.FullName = $"Deleted User {contact.Id}";
                contact.DeletedAt = DateTime.UtcNow;
                await UpdateAsync(contact);
        }
}