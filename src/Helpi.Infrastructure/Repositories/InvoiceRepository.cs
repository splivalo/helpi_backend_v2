namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class InvoiceRepository : IInvoiceRepository
{
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context) => _context = context;

        public async Task<Invoice> GetByIdAsync(int id)
            => await _context.Invoices
                .Include(i => i.JobInstance)
                .Include(i => i.Transaction)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
            => await _context.Invoices
                .Where(i => i.Status == status)
                .ToListAsync();

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
            => await _context.Invoices
                .Where(i => i.DueDate < DateOnly.FromDateTime(DateTime.UtcNow) &&
                    i.Status != InvoiceStatus.Paid)
                .ToListAsync();

        public async Task<Invoice> AddAsync(Invoice invoice)
        {
                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
                return invoice;
        }

        public async Task UpdateAsync(Invoice invoice)
        {
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Invoice invoice)
        {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
        }
}