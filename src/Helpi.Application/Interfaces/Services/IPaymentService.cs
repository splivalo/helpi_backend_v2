using Stripe;

namespace Helpi.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task HandlePaymentRefund(string paymentIntentId, string refundId, decimal refundAmount, string refundReaso);

        Task ProcessPaymentAsync(int jobInstanceId);
        Task<bool> RetryInvoiceAsync(int transactionId);
        Task RetryFailedInvoicesAsync();
    }
}