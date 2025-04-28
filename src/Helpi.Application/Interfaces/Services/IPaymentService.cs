using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        ///  methods for making payments

        Task<string> CreateCustomerAsync(User user);
        Task<string> SavePaymentMethodAsync(string customerId, string paymentMethodId);
        Task<string> ChargePaymentAsync(Order order, User user);
        Task<IEnumerable<PaymentMethodDto>> GetSavedPaymentMethodsAsync(string customerId);


        /// methods for receiving payments

        // Task<string> CreateConnectAccountAsync(User user);
        // Task<bool> UpdateConnectAccountDetailsAsync(string connectAccountId, ConnectAccountDetails details);
        // Task<PayoutResponse> CreatePayoutAsync(User user, decimal amount);
        // Task<IEnumerable<PayoutResponse>> GetPayoutHistoryAsync(string connectAccountId);
    }
}