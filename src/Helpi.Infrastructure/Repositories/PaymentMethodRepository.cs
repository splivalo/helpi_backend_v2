namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PaymentMethodRepository : IPaymentMethodRepository
{
        private readonly AppDbContext _context;

        public PaymentMethodRepository(AppDbContext context) => _context = context;

        public async Task<PaymentMethod> GetByIdAsync(int id)
            => await _context.PaymentMethods.FindAsync(id);

        public async Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(int userId)
            => await _context.PaymentMethods
                .Where(pm => pm.UserId == userId)
                .ToListAsync();

        public async Task<PaymentMethod> GetDefaultPaymentMethodAsync(int userId)
            => await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault);

        public async Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod)
        {
                await _context.PaymentMethods.AddAsync(paymentMethod);
                await _context.SaveChangesAsync();
                return paymentMethod;
        }

        public async Task UpdateAsync(PaymentMethod paymentMethod)
        {
                _context.PaymentMethods.Update(paymentMethod);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PaymentMethod paymentMethod)
        {
                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
        }
}