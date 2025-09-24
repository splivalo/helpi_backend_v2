using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PaymentProfileRepository : IPaymentProfileRepository
{
    private readonly AppDbContext _context;

    public PaymentProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProfile?> GetStipePaymentByUserIdAsync(int userId)
    {
        return await _context.PaymentProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId
            && p.PaymentProcessor == PaymentProcessor.Stripe);
    }

    public async Task<PaymentProfile?> GetByIdAsync(int id)
    {
        return await _context.PaymentProfiles.FindAsync(id);
    }

    public async Task<PaymentProfile?> AddAsync(PaymentProfile paymentProfile)
    {
        await _context.PaymentProfiles.AddAsync(paymentProfile);
        await _context.SaveChangesAsync();
        return paymentProfile;
    }

    public async Task UpdateAsync(PaymentProfile paymentProfile)
    {
        _context.PaymentProfiles.Update(paymentProfile);
        await _context.SaveChangesAsync();
    }

}
