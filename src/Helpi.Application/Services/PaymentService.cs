// // Application/Services/PaymentService.cs
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

public class PaymentService : IPaymentService
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly IJobInstanceRepository _jobInstanceRepo;
    private readonly IPaymentProfileRepository _paymentProfileRepo;
    private readonly IStripePaymentService _stripePaymentService;

    public PaymentService(
        IPaymentTransactionRepository transactionRepository,
       IJobInstanceRepository jobInstanceRepo,
       IPaymentProfileRepository paymentProfileRepo,
        IStripePaymentService stripePaymentService
        )
    {
        _transactionRepository = transactionRepository;
        _jobInstanceRepo = jobInstanceRepo;
        _paymentProfileRepo = paymentProfileRepo;
        _stripePaymentService = stripePaymentService;
    }



    public async Task ProcessPaymentAsync(int jobInstanceId)
    {

        var jobInstance = await _jobInstanceRepo.LoadJobInstanceWithIncludes(jobInstanceId, new JobInstanceIncludeOptions
        {
            Order = true
        });


        if (jobInstance == null)
        {
            return;
        }

        if (jobInstance.Status == JobInstanceStatus.Cancelled)
        {
            return;
        }



        if (jobInstance.Order.PaymentMethodId == null)
        {
            return;
        }


        var transaction = new PaymentTransaction
        {
            Status = PaymentStatus.Pending
        };

        transaction.JobInstanceId = jobInstanceId;
        transaction.OrderId = jobInstance.OrderId;
        transaction.ScheduledAt = DateTime.UtcNow;
        transaction.ProcessedAt = DateTime.UtcNow;
        transaction.Amount = jobInstance.TotalAmount;
        transaction.CustomerId = jobInstance.CustomerId;
        transaction.PaymentMethodId = (int)jobInstance.Order.PaymentMethodId!;
        transaction.IdempotencyKey = $"order-{transaction.OrderId}-job-{transaction.JobInstanceId}-{transaction.Id}";

        await _transactionRepository.AddAsync(transaction);
        try
        {
            var paymentProfile = await _paymentProfileRepo.GetStipePaymentByUserIdAsync(jobInstance.CustomerId);

            if (paymentProfile == null)
            {
                transaction.Status = PaymentStatus.Failed;
                return;
            }

            var stripeCustomerId = paymentProfile.StripeCustomerId;

            if (stripeCustomerId == null)
            {
                transaction.Status = PaymentStatus.Failed;
                return;
            }

            var paymentResult = await _stripePaymentService.ChargePaymentAsync(stripeCustomerId!, transaction);

            transaction.Status = paymentResult.Success ? PaymentStatus.Paid : PaymentStatus.Failed;
        }
        catch
        {
            transaction.Status = PaymentStatus.Failed;
        }

        await _transactionRepository.UpdateAsync(transaction);
    }
}