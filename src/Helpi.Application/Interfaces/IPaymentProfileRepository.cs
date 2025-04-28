
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;
public interface IPaymentProfileRepository
{
    Task<PaymentProfile?> GetStipePaymentByUserIdAsync(int userId);
    Task<PaymentProfile?> GetByIdAsync(int id);
    Task AddAsync(PaymentProfile paymentProfile);
    Task UpdateAsync(PaymentProfile paymentProfile);
}
