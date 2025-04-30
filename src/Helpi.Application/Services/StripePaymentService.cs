using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Helpi.Application.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentProfileRepository _paymentProfileRepository;

        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentService> _logger;

        private readonly IMapper _mapper;

        public StripePaymentService(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IPaymentProfileRepository paymentProfileRepository,
            IConfiguration configuration,
            ILogger<StripePaymentService> logger,
             IMapper mapper
            )
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _paymentProfileRepository = paymentProfileRepository;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;




            // Initialize Stripe API with your secret key


            var stripeSecretKey = Environment.GetEnvironmentVariable("Stripe:SecretKey")
                                  ??
                                  configuration["Stripe:SecretKey"];



            StripeConfiguration.ApiKey = stripeSecretKey;

        }

        public async Task<string> CreateCustomerAsync(User user)
        {
            try
            {

                var fullName = user.Email;

                if (user.UserType == UserType.Customer)
                {
                    fullName = user?.Customer?.Contact.FullName;
                }
                else if (user.UserType == UserType.Student)
                {
                    fullName = user?.Student?.Contact.FullName;
                }


                var customerOptions = new CustomerCreateOptions
                {
                    Email = user!.Email,
                    Name = fullName ?? "",
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", user.Id.ToString() }
                    }
                };

                var customerService = new Stripe.CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                return customer.Id;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer for user {UserId}", user.Id);
                throw new ApplicationException("Failed to create payment profile", ex);
            }
        }

        public async Task<string> SavePaymentMethodAsync(string StripeCustomerId, string paymentMethodId)
        {
            try
            {
                var stripePaymentMethodService = new Stripe.PaymentMethodService();

                // Attach the payment method to the customer
                await stripePaymentMethodService.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
                {
                    Customer = StripeCustomerId
                });

                // Set as default payment method
                var customerService = new Stripe.CustomerService();
                await customerService.UpdateAsync(StripeCustomerId, new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                });

                return paymentMethodId;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error saving payment method {PaymentMethodId} for customer {CustomerId}", paymentMethodId, StripeCustomerId);
                throw new ApplicationException("Failed to save payment method", ex);
            }
        }

        public async Task<string> ChargePaymentAsync(Order order, User user)
        {
            try
            {
                // Get the total amount in cents
                // long amountInCents = (long)(order.TotalAmount * 100);

                // var paymentIntentService = new PaymentIntentService();
                // var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                // {
                //     Amount = amountInCents,
                //     Currency = "usd",
                //     Customer = user.StripeCustomerId,
                //     Description = $"Order #{order.Id} - {DateTime.UtcNow}",
                //     Metadata = new Dictionary<string, string>
                //     {
                //         { "OrderId", order.Id.ToString() }
                //     },
                //     CaptureMethod = "automatic", // Automatically capture the payment
                //     ConfirmationMethod = "automatic",
                //     Confirm = true // Confirm and process immediately
                // });

                // if (paymentIntent.Status == "succeeded")
                // {
                //     return paymentIntent.Id;
                // }
                // else if (paymentIntent.Status == "requires_action")
                // {
                //     throw new ApplicationException("This payment requires additional authentication");
                // }
                // else
                // {
                //     throw new ApplicationException($"Payment failed with status: {paymentIntent.Status}");
                // }

                return "not implemented";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error charging payment for order {OrderId}", order.Id);
                throw new ApplicationException("Payment processing failed", ex);
            }
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetSavedPaymentMethodsAsync(string stripeCustomerId)
        {
            try
            {
                var paymentMethodService = new Stripe.PaymentMethodService();
                var options = new PaymentMethodListOptions
                {
                    Customer = stripeCustomerId,
                    Type = "card"
                };

                var paymentMethods = await paymentMethodService.ListAsync(options);
                var response = new List<PaymentMethodDto>();

                foreach (var method in paymentMethods)
                {
                    response.Add(new PaymentMethodDto
                    {
                        Id = -1, // this is in our DB
                        ProcessorToken = method.Id,
                        Brand = method.Card.Brand,
                        Last4 = method.Card.Last4,
                        ExpiryMonth = (int?)method.Card.ExpMonth,
                        ExpiryYear = (int?)method.Card.ExpYear,
                        IsDefault = method.Id == await GetDefaultPaymentMethodIdAsync(stripeCustomerId)
                    });
                }

                return response;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods for customer {CustomerId}", stripeCustomerId);
                throw new ApplicationException("Failed to retrieve saved payment methods", ex);
            }
        }

        private async Task<string?> GetDefaultPaymentMethodIdAsync(string customerId)
        {
            var customerService = new Stripe.CustomerService();
            var customer = await customerService.GetAsync(customerId);
            return customer.InvoiceSettings?.DefaultPaymentMethod?.Id;
        }


        public async Task<string> CreateSetupIntentAsync(User user)
        {


            var stripePaymentProfile = await _paymentProfileRepository.GetStipePaymentByUserIdAsync(user.Id);

            // Ensure user has Stripe payment profile
            if (stripePaymentProfile == null)
            {
                var stripeCustomerId = await CreateCustomerAsync(user);

                var createPaymentProfile = new CreatePaymentProfileDto
                {
                    UserId = user.Id,
                    StripeCustomerId = stripeCustomerId,
                    PaymentProcessor = PaymentProcessor.Stripe
                };

                var paymentProfilefile = _mapper.Map<PaymentProfile>(createPaymentProfile);


                await _paymentProfileRepository.AddAsync(paymentProfilefile);
            }

            // Create a SetupIntent to securely collect payment details
            var setupIntentService = new SetupIntentService();
            var setupIntent = await setupIntentService.CreateAsync(new SetupIntentCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Customer = stripePaymentProfile!.StripeCustomerId,
                Usage = "off_session" // Required for future payments without customer
            });

            var ClientSecret = setupIntent.ClientSecret;

            return ClientSecret;

        }


    }
}