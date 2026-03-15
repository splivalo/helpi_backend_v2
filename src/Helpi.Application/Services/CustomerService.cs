
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class CustomerService
{
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;
        private readonly IFirebaseService _firebaseService;
        private readonly IUserRepository _userRepository;
        private readonly SeniorService _seniorService;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IMinimaxService _minimaxService;
        private readonly FcmTokensService _fcmTokensService;
        private readonly INotificationService _notificationService;
        private readonly INotificationFactory _notificationFactory;
        private readonly IContactInfoRepository _contactInfoRepo;
        private readonly IPaymentProfileRepository _paymentProfileRepository;

        public CustomerService(
                ICustomerRepository repository,
                IMapper mapper,
                ILogger<CustomerService> logger,
                IFirebaseService firebaseService,
                IUserRepository userRepository,
                SeniorService seniorService,
                IStripePaymentService stripePaymentService,
                IMinimaxService minimaxService,
                FcmTokensService fcmTokensService,
                INotificationService notificationService,
                INotificationFactory notificationFactory,
                IContactInfoRepository contactInfoRepo,
                IPaymentProfileRepository paymentProfileRepository)
        {
                _repository = repository;
                _mapper = mapper;
                _logger = logger;
                _firebaseService = firebaseService;
                _userRepository = userRepository;
                _seniorService = seniorService;
                _stripePaymentService = stripePaymentService;
                _minimaxService = minimaxService;
                _fcmTokensService = fcmTokensService;
                _notificationService = notificationService;
                _notificationFactory = notificationFactory;
                _contactInfoRepo = contactInfoRepo;
                _paymentProfileRepository = paymentProfileRepository;
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
                var customers = await _repository.GetAllCustomersAsync();

                return _mapper.Map<List<CustomerDto>>(customers);

        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto)
        {
                var customer = _mapper.Map<Customer>(dto);
                await _repository.AddAsync(customer);
                return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
                var customer = await _repository.GetByIdAsync(id);

                if (customer == null)
                {
                        throw new NotFoundException(nameof(Customer), id);
                }

                return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
                _logger.LogInformation("🗑️ Deleting customer {CustomerId}", customerId);

                try
                {
                        // Step 1: Get customer with seniors and contact
                        var customer = await _repository.LoadCustomerWithIncludes(customerId, new CustomerIncludeOptions
                        {
                                Contact = true,
                                Seniors = true
                        });

                        if (customer == null)
                        {
                                _logger.LogWarning("⚠️ Customer {CustomerId} not found", customerId);
                                return false;
                        }

                        var originalName = customer.Contact?.FullName ?? $"Customer {customerId}";

                        // Step 2: Delete all associated seniors
                        _logger.LogInformation("🔄 Deleting {Count} seniors for customer {CustomerId}", customer.Seniors.Count, customerId);
                        foreach (var senior in customer.Seniors)
                        {
                                await _seniorService.DeleteSeniorAsync(senior.Id);
                        }
                        _logger.LogInformation("✅ All seniors deleted for customer {CustomerId}", customerId);

                        // Step 3: Cleanup Stripe (non-blocking on failure)
                        try
                        {
                                var paymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(customerId);
                                if (paymentProfile?.StripeCustomerId != null)
                                {
                                        _logger.LogInformation("💳 Cleaning up Stripe for customer {CustomerId}", customerId);
                                        await _stripePaymentService.DeletePaymentMethodsForCustomerAsync(paymentProfile.StripeCustomerId);
                                        await _stripePaymentService.AnonymizeStripeCustomerAsync(paymentProfile.StripeCustomerId);
                                }
                        }
                        catch (Exception stripeEx)
                        {
                                _logger.LogError(stripeEx, "⚠️ Failed to cleanup Stripe for customer {CustomerId}, continuing with deletion", customerId);
                        }

                        // Step 4: Cleanup Minimax (placeholder, non-blocking)
                        try
                        {
                                var paymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(customerId);
                                if (paymentProfile?.MinimaxCustomerId != null)
                                {
                                        _logger.LogInformation("📋 Cleaning up Minimax for customer {CustomerId}", customerId);
                                        await _minimaxService.AnonymizeCustomerAsync(paymentProfile.MinimaxCustomerId.Value);
                                }
                        }
                        catch (Exception minimaxEx)
                        {
                                _logger.LogError(minimaxEx, "⚠️ Failed to cleanup Minimax for customer {CustomerId}, continuing with deletion", customerId);
                        }

                        // Step 5: Delete FCM tokens (non-blocking)
                        try
                        {
                                _logger.LogInformation("🗑️ Deleting FCM tokens for customer {CustomerId}", customerId);
                                await _fcmTokensService.DeleteUserFcmTokensAsync(customerId);
                        }
                        catch (Exception fcmEx)
                        {
                                _logger.LogError(fcmEx, "⚠️ Failed to delete FCM tokens for customer {CustomerId}, continuing with deletion", customerId);
                        }

                        // Step 6: Anonymize Firebase data (non-blocking)
                        try
                        {
                                _logger.LogInformation("🔥 Anonymizing Firebase data for customer {CustomerId}", customerId);
                                await _firebaseService.AnonymizeAndLogoutUserAsync(customerId);
                        }
                        catch (Exception firebaseEx)
                        {
                                _logger.LogError(firebaseEx, "⚠️ Failed to anonymize Firebase data for customer {CustomerId}, continuing with deletion", customerId);
                        }

                        // Step 7: Anonymize Identity data
                        _logger.LogInformation("🔐 Anonymizing Identity data for customer {CustomerId}", customerId);
                        _ = await _userRepository.AnonymizeAndLogoutUserAsync(customerId);
                        _logger.LogInformation("✅ Identity data anonymized for customer {CustomerId}", customerId);

                        // Step 8: Anonymize customer contact info
                        if (customer.Contact != null)
                        {
                                _logger.LogInformation("🔐 Anonymizing contact info for customer {CustomerId}", customerId);
                                await _contactInfoRepo.AnonymizeContactAsync(customer.Contact);
                        }

                        // step 9: soft delete the customer
                        customer.DeletedAt = DateTime.UtcNow;
                        await _repository.UpdateAsync(customer);
                        _logger.LogInformation("✅ Customer {CustomerId} soft deleted successfully", customerId);

                        // Step 10: Send admin notification
                        try
                        {
                                _logger.LogInformation("📧 Sending deletion notification to admin for customer {CustomerId}", customerId);
                                var notification = _notificationFactory.UserDeletedNotification(
                                        receiverUserId: 1, // admin
                                        deletedUserId: customerId,
                                        deletedUserName: originalName,
                                        NotificationType.CustomerDeleted
                                );
                                await _notificationService.StoreAndNotifyAsync(notification);
                                _logger.LogInformation("✅ Admin notification sent for deleted customer {CustomerId}", customerId);
                        }
                        catch (Exception notifyEx)
                        {
                                _logger.LogError(notifyEx, "⚠️ Failed to send deletion notification for customer {CustomerId}, but deletion completed", customerId);
                        }

                        _logger.LogInformation("✅ Customer {CustomerId} deleted successfully", customerId);
                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to delete customer {CustomerId}", customerId);
                        return false;
                }
        }
}