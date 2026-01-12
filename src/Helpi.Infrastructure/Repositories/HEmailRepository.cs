namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class HEmailRepository : IHEmailRepository
{
        private readonly AppDbContext _context;

        public HEmailRepository(AppDbContext context) => _context = context;

        public async Task<HEmail> GetByIdAsync(int id)
            => await _context.InvoiceEmails
                .FirstOrDefaultAsync(ie => ie.Id == id);

        public async Task<IEnumerable<HEmail>> GetFailedEmailsAsync()
            => await _context.InvoiceEmails
                .Where(ie => ie.Status == EmailStatus.Failed &&
                    ie.AttemptCount < 5)
                .ToListAsync();

        public async Task<HEmail> AddAsync(HEmail email)
        {
                await _context.InvoiceEmails.AddAsync(email);
                await _context.SaveChangesAsync();
                return email;
        }

        public async Task UpdateAsync(HEmail email)
        {
                _context.InvoiceEmails.Update(email);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(HEmail email)
        {
                _context.InvoiceEmails.Remove(email);
                await _context.SaveChangesAsync();
        }
}