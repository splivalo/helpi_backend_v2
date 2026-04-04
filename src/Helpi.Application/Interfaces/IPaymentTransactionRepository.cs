using Helpi.Domain.Entities;
using Stripe;

namespace Helpi.Application.Interfaces;

public interface IPaymentTransactionRepository
{
    Task<PaymentTransaction?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentTransaction>> GetPendingTransactionsAsync();
    Task<IEnumerable<PaymentTransaction>> GetFailedTransactionsAsync();
    Task<PaymentTransaction> AddAsync(PaymentTransaction transaction);
    Task UpdateAsync(PaymentTransaction transaction);
    Task DeleteAsync(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task<IEnumerable<PaymentTransaction>> GetFailedInvoiceTransactionsAsync();

}
