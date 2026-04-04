using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(int customerId);
    Task<PaymentMethod?> GetDefaultPaymentMethodAsync(int customerId);
    Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod);
    Task AddNoSaveAsync(PaymentMethod paymentMethod);
    Task UpdateAsync(PaymentMethod paymentMethod);
    Task UpdateNoSaveAsync(PaymentMethod paymentMethod);
    Task DeleteAsync(PaymentMethod paymentMethod);
    Task SaveAsync();
}
