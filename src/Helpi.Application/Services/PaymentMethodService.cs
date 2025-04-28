
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class PaymentMethodService
{
        private readonly IPaymentMethodRepository _repository;
        private readonly IMapper _mapper;

        public PaymentMethodService(IPaymentMethodRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<PaymentMethodDto>> GetMethodsByCustomerAsync(int customerId)
        {

                var methods = await _repository.GetByUserIdAsync(customerId);
                return _mapper.Map<List<PaymentMethodDto>>(methods);
        }

        public async Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodCreateDto dto)
        {
                var method = _mapper.Map<PaymentMethod>(dto);
                await _repository.AddAsync(method);
                return _mapper.Map<PaymentMethodDto>(method);
        }

        /// <summary>
        /// Sync a specific  payment method with Stripe
        /// </summary>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        public async Task SyncStripePaymentMethodWithStripeAsync(int paymentMethodId)
        {
                // var paymentMethod = await _dbContext.PaymentMethods.FindAsync(paymentMethodId);
                // if (paymentMethod == null) return;

                // try
                // {
                //         var stripePaymentMethod = await _stripeClient.PaymentMethods.GetAsync(paymentMethod.StripePaymentMethodId);

                //         // Update local cache
                //         paymentMethod.Brand = stripePaymentMethod.Card.Brand;
                //         paymentMethod.Last4 = stripePaymentMethod.Card.Last4;
                //         paymentMethod.ExpiryMonth = stripePaymentMethod.Card.ExpMonth;
                //         paymentMethod.ExpiryYear = stripePaymentMethod.Card.ExpYear;
                //         paymentMethod.LastSyncedAt = DateTime.UtcNow;

                //         await _dbContext.SaveChangesAsync();
                // }
                // catch (StripeException ex) when (ex.StripeError.Code == "resource_missing")
                // {
                //         // Handle case where payment method was deleted in Stripe
                //         paymentMethod.IsActive = false;
                //         await _dbContext.SaveChangesAsync();
                // }
        }

        /// <summary>
        /// Sync Stripe all payment methods to our DB for a customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task SyncAllCustomerPaymentMethodsWithStripeAsync(int customerId)
        {
                // var customer = await _dbContext.Customers
                //     .Include(c => c.PaymentMethods)
                //     .FirstOrDefaultAsync(c => c.Id == customerId);

                // if (customer == null) return;

                // // Get all payment methods from Stripe
                // var options = new PaymentMethodListOptions
                // {
                //         Customer = customer.StripeCustomerId,
                //         Type = "card"
                // };

                // var stripePaymentMethods = await _stripeClient.PaymentMethods.ListAsync(options);

                // // Track which ones we've processed
                // var processedIds = new HashSet<string>();

                // // Update existing payment methods and add new ones
                // foreach (var stripePM in stripePaymentMethods)
                // {
                //         processedIds.Add(stripePM.Id);

                //         var localPM = customer.PaymentMethods
                //             .FirstOrDefault(pm => pm.StripePaymentMethodId == stripePM.Id);

                //         if (localPM != null)
                //         {
                //                 // Update existing
                //                 localPM.Brand = stripePM.Card.Brand;
                //                 localPM.Last4 = stripePM.Card.Last4;
                //                 localPM.ExpiryMonth = stripePM.Card.ExpMonth;
                //                 localPM.ExpiryYear = stripePM.Card.ExpYear;
                //                 localPM.IsActive = true;
                //                 localPM.LastSyncedAt = DateTime.UtcNow;
                //         }
                //         else
                //         {
                //                 // Add new
                //                 _dbContext.PaymentMethods.Add(new PaymentMethod
                //                 {
                //                         CustomerId = customerId,
                //                         StripePaymentMethodId = stripePM.Id,
                //                         Brand = stripePM.Card.Brand,
                //                         Last4 = stripePM.Card.Last4,
                //                         ExpiryMonth = stripePM.Card.ExpMonth,
                //                         ExpiryYear = stripePM.Card.ExpYear,
                //                         IsActive = true
                //                 });
                //         }
                // }

                // // Mark payment methods that no longer exist in Stripe as inactive
                // foreach (var paymentMethod in customer.PaymentMethods)
                // {
                //         if (!processedIds.Contains(paymentMethod.StripePaymentMethodId))
                //         {
                //                 paymentMethod.IsActive = false;
                //         }
                // }

                // await _dbContext.SaveChangesAsync();
        }
}