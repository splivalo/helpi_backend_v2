using Helpi.Application.DTOs;
using Helpi.Domain.Entities;
using Helpi.Domain.ValueObjects;

namespace Helpi.Application.Interfaces.Services
{
    public interface IStripePaymentService
    {
        ///  methods for making payments

        Task<string> CreateSetupIntentAsync(User user);
        Task<string> CreateCustomerAsync(User user);
        Task<string> SavePaymentMethodAsync(string customerId, string paymentMethodId);
        Task<PaymentResult> ChargePaymentAsync(string stripeCustomerId, PaymentTransaction transaction);
        Task<IEnumerable<PaymentMethodDto>> GetSavedPaymentMethodsAsync(string customerId);

        /// cleanup methods for account deletion
        Task DeletePaymentMethodsForCustomerAsync(string stripeCustomerId);
        Task AnonymizeStripeCustomerAsync(string stripeCustomerId);

        /// methods for receiving payments

        // Task<string> CreateConnectAccountAsync(User user);
        // Task<bool> UpdateConnectAccountDetailsAsync(string connectAccountId, ConnectAccountDetails details);
        // Task<PayoutResponse> CreatePayoutAsync(User user, decimal amount);
        // Task<IEnumerable<PayoutResponse>> GetPayoutHistoryAsync(string connectAccountId);
    }
}