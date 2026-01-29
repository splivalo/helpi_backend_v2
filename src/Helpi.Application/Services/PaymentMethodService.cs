
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Stripe;

namespace Helpi.Application.Services;

public class PaymentMethodService
{
        private readonly IPaymentMethodRepository _repository;
        private readonly IMapper _mapper;

        private readonly IPaymentProfileRepository _paymentProfileRepository;

        public PaymentMethodService(IPaymentMethodRepository repository, IMapper mapper, IPaymentProfileRepository paymentProfileRepository)
        {
                _repository = repository;
                _paymentProfileRepository = paymentProfileRepository;
                _mapper = mapper;
        }

        public async Task<List<PaymentMethodDto>> GetMethodsByUserIdAsync(int userId)
        {

                var methods = await _repository.GetByUserIdAsync(userId);
                return _mapper.Map<List<PaymentMethodDto>>(methods);
        }

        public async Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodCreateDto dto)
        {
                var method = _mapper.Map<Domain.Entities.PaymentMethod>(dto);
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
        public async Task SyncAllPaymentMethodsWithStripeForCustomerAsync(int userId)
        {

                var localPaymentMethods = await _repository.GetByUserIdAsync(userId);
                var stripePaymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(userId);

                if (stripePaymentProfile == null) return;

                var stripeCustomerId = stripePaymentProfile.StripeCustomerId;

                // Get all payment methods from Stripe
                var options = new PaymentMethodListOptions
                {
                        Customer = stripeCustomerId,
                        Type = "card"
                };

                var service = new Stripe.PaymentMethodService();

                var stripePaymentMethods = await service.ListAsync(options);

                // Track which ones we've processed
                var processedIds = new HashSet<string>();

                // Update existing payment methods and add new ones
                foreach (var stripePM in stripePaymentMethods)
                {
                        processedIds.Add(stripePM.Id);

                        var localPM = localPaymentMethods
                            .FirstOrDefault(pm => pm.ProcessorToken == stripePM.Id);

                        if (localPM != null)
                        {
                                // Update existing
                                localPM.ProcessorToken = stripePM.Id;
                                localPM.Brand = stripePM.Card.Brand;
                                localPM.Last4 = stripePM.Card.Last4;
                                localPM.ExpiryMonth = (int?)stripePM.Card.ExpMonth;
                                localPM.ExpiryYear = (int?)stripePM.Card.ExpYear;
                                localPM.IsDeleted = false;

                                await _repository.UpdateNoSaveAsync(localPM);
                        }
                        else

                        {
                                var newPm = new Domain.Entities.PaymentMethod
                                {
                                        UserId = userId,
                                        PaymentProcessor = PaymentProcessor.Stripe,
                                        ProcessorToken = stripePM.Id,
                                        Brand = stripePM.Card.Brand,
                                        Last4 = stripePM.Card.Last4,
                                        ExpiryMonth = (int?)stripePM.Card.ExpMonth,
                                        ExpiryYear = (int?)stripePM.Card.ExpYear,
                                        IsDeleted = false
                                };


                                await _repository.AddNoSaveAsync(newPm);
                        }
                }

                // Mark payment methods that no longer exist in Stripe as deleted and anonymize
                foreach (var paymentMethod in localPaymentMethods)
                {
                        if (!processedIds.Contains(paymentMethod.ProcessorToken!))
                        {
                                paymentMethod.IsDeleted = true;
                                paymentMethod.DeletedAt = DateTime.UtcNow;
                                paymentMethod.Last4 = "****";
                                paymentMethod.Brand = "deleted";
                                paymentMethod.ProcessorToken = $"deleted_{paymentMethod.Id}";
                                await _repository.UpdateNoSaveAsync(paymentMethod);
                        }
                }

                await _repository.SaveAsync();
        }
}