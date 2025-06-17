namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe;


public class PaymentTransactionRepository : IPaymentTransactionRepository
{
        private readonly AppDbContext _context;

        public PaymentTransactionRepository(AppDbContext context) => _context = context;

        public async Task<PaymentTransaction> GetByIdAsync(int id)
            => await _context.PaymentTransactions
                .Include(pt => pt.JobInstance)
                .Include(pt => pt.Customer)
                .Include(pt => pt.PaymentMethod)
                .FirstOrDefaultAsync(pt => pt.Id == id);

        public async Task<IEnumerable<PaymentTransaction>> GetPendingTransactionsAsync()
            => await _context.PaymentTransactions
                .Where(pt => pt.Status == PaymentStatus.Pending)
                .ToListAsync();

        public async Task<IEnumerable<PaymentTransaction>> GetFailedTransactionsAsync()
            => await _context.PaymentTransactions
                .Where(pt => pt.Status == PaymentStatus.Failed && pt.RetryCount < pt.MaxRetries)
                .ToListAsync();

        public async Task<PaymentTransaction> AddAsync(PaymentTransaction transaction)
        {
                await _context.PaymentTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return transaction;
        }

        public async Task UpdateAsync(PaymentTransaction transaction)
        {
                _context.PaymentTransactions.Update(transaction);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PaymentTransaction transaction)
        {
                _context.PaymentTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
        }

        public async Task<PaymentTransaction?> GetByPaymentIntentIdAsync(string paymentIntentId)
        {
                return await _context.PaymentTransactions
                          .Include(pt => pt.JobInstance)
                          .FirstOrDefaultAsync(pt => pt.ProcessPaymentId == paymentIntentId);
        }

}