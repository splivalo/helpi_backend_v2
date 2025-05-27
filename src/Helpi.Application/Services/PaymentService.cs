// // Application/Services/PaymentService.cs
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

public class PaymentService : IPaymentService
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly IJobInstanceRepository _jobInstanceRepo;
    private readonly IPaymentProfileRepository _paymentProfileRepo;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentTransactionRepository transactionRepository,
       IJobInstanceRepository jobInstanceRepo,
       IPaymentProfileRepository paymentProfileRepo,
        IStripePaymentService stripePaymentService,
      ILogger<PaymentService> logger
        )
    {
        _transactionRepository = transactionRepository;
        _jobInstanceRepo = jobInstanceRepo;
        _paymentProfileRepo = paymentProfileRepo;
        _stripePaymentService = stripePaymentService;
        _logger = logger;
    }



    public async Task ProcessPaymentAsync(int jobInstanceId)
    {
        var jobInstance = await _jobInstanceRepo.LoadJobInstanceWithIncludes(jobInstanceId, new JobInstanceIncludeOptions
        {
            Order = true,
            OrderPaymentMethod = true
        });

        if (jobInstance == null)
        {
            _logger.LogWarning("❌ JobInstance {JobInstanceId} not found", jobInstanceId);
            return;
        }

        if (jobInstance.Status == JobInstanceStatus.Cancelled)
        {
            _logger.LogInformation("🚫 JobInstance {JobInstanceId} is cancelled. Skipping payment.", jobInstanceId);
            return;
        }

        if (jobInstance.Order?.PaymentMethodId == null)
        {
            _logger.LogWarning("⚠️ JobInstance {JobInstanceId} has no payment method", jobInstanceId);
            return;
        }

        var transaction = new PaymentTransaction
        {
            JobInstanceId = jobInstanceId,
            OrderId = jobInstance.OrderId,
            ScheduledAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            Amount = jobInstance.TotalAmount,
            CustomerId = jobInstance.CustomerId,
            PaymentMethodId = (int)jobInstance.Order.PaymentMethodId,
            PaymentMethod = jobInstance.Order.PaymentMethod,
            Status = PaymentStatus.Pending,
            IdempotencyKey = $"order-{jobInstance.OrderId}-job-{jobInstanceId}"
        };

        await _transactionRepository.AddAsync(transaction);

        try
        {
            var paymentProfile = await _paymentProfileRepo.GetStipePaymentByUserIdAsync(jobInstance.CustomerId);
            if (paymentProfile?.StripeCustomerId == null)
            {
                _logger.LogWarning("❗ Missing Stripe profile or customer ID for user {CustomerId}", jobInstance.CustomerId);
                transaction.Status = PaymentStatus.Failed;
            }
            else
            {
                var result = await _stripePaymentService.ChargePaymentAsync(paymentProfile.StripeCustomerId, transaction);

                if (result.Success)
                {
                    transaction.Status = PaymentStatus.Paid;
                    _logger.LogInformation("✅ Payment successful for JobInstance {JobInstanceId} 💸", jobInstanceId);
                }
                else
                {
                    transaction.Status = PaymentStatus.Failed;
                    _logger.LogWarning("❌ Payment failed for JobInstance {JobInstanceId}: {Error}", jobInstanceId, result.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔥 Exception while processing payment for JobInstance {JobInstanceId}", jobInstanceId);
            transaction.Status = PaymentStatus.Failed;
        }

        await _transactionRepository.UpdateAsync(transaction);
    }

}