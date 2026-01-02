
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.Minimax;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;


public class PaymentService : IPaymentService
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly IJobInstanceRepository _jobInstanceRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IPaymentProfileRepository _paymentProfileRepo;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IMinimaxService _minimaxService;
    private readonly ILogger<PaymentService> _logger;

    private readonly INotificationService _notificationService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IMailgunService _mailgunService;
    private readonly IHEmailRepository _hEmailRepository;

    private readonly ILocalizationService _loc;

    public PaymentService(
        IPaymentTransactionRepository transactionRepository,
       IJobInstanceRepository jobInstanceRepo,
       ICustomerRepository customerRepo,
       IPaymentProfileRepository paymentProfileRepo,
        IStripePaymentService stripePaymentService,
      IMinimaxService minimaxService,
      ILogger<PaymentService> logger,
   INotificationService notificationService,
INotificationFactory notificationFactory,
 IMailgunService mailgunService,

IHEmailRepository hEmailRepository,
ILocalizationService loc
        )
    {
        _transactionRepository = transactionRepository;
        _jobInstanceRepo = jobInstanceRepo;
        _customerRepo = customerRepo;
        _paymentProfileRepo = paymentProfileRepo;
        _stripePaymentService = stripePaymentService;
        _minimaxService = minimaxService;
        _logger = logger;
        _notificationService = notificationService;
        _notificationFactory = notificationFactory;
        _mailgunService = mailgunService;
        _hEmailRepository = hEmailRepository;
        _loc = loc;
    }


    public async Task ProcessPaymentAsync(int jobInstanceId)
    {
        var jobInstance = await _jobInstanceRepo.LoadJobInstanceWithIncludes(jobInstanceId, new JobInstanceIncludeOptions
        {
            Order = true,
            OrderPaymentMethod = true,
            Assignment = true,
            AssignmentStudent = true,
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

        var activeStudent = new[]
        {
            StudentStatus.Active,
            StudentStatus.ContractAboutToExpire,
        };

        var student = jobInstance!.ScheduleAssignment!.Student;
        var studentStatus = student.Status;

        if (!activeStudent.Contains(studentStatus))
        {
            _logger.LogInformation("🚫 JobInstance {JobInstanceId} Student {studentId} has status {status}", jobInstanceId, student.UserId, studentStatus);
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

        PaymentProfile? paymentProfile = null;

        try
        {
            paymentProfile = await _paymentProfileRepo.GetStipePaymentByUserIdAsync(jobInstance.CustomerId);
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
                    transaction.ProcessPaymentId = result.PaymentIntentId;
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


        jobInstance.PaymentTransactionId = transaction.Id;
        jobInstance.PaymentStatus = transaction.Status;
        await _jobInstanceRepo.UpdateAsync(jobInstance);


        /// 
        /// ---  Minimax ------
        /// 

        if (transaction.Status != PaymentStatus.Paid)
        {
            await HandlePaymentFailed(jobInstance);
            return;
        }

        await HandlePaymentSuccess(jobInstance, paymentProfile!);


    }




    public async Task HandlePaymentRefund(string paymentIntentId, string refundId, decimal refundAmount, string refundReason)
    {


        var transaction = await _transactionRepository.GetByPaymentIntentIdAsync(paymentIntentId);

        if (transaction == null)
        {
            _logger.LogInformation("❌ [Failed]  to get PaymentTransaction with paymentIntentId: {paymentIntentId}", paymentIntentId);
            return;
        }

        transaction.Status = PaymentStatus.Refunded;
        transaction.RefundId = refundId;
        transaction.RefundAmount = refundAmount;
        transaction.RefundReason = refundReason;
        transaction.RefundedAt = DateTime.UtcNow;
        //
        transaction.JobInstance.Status = JobInstanceStatus.Cancelled;
        transaction.JobInstance.PaymentStatus = PaymentStatus.Refunded;

        await _transactionRepository.UpdateAsync(transaction);

        _logger.LogInformation("✅ [Pass]  Refunded paymentIntentId: {paymentIntentId}", paymentIntentId);



    }

    private async Task HandlePaymentFailed(JobInstance jobInstance)
    {
        try
        {
            var adminId = 1;
            // notify admin
            var notification = _notificationFactory.CreatePaymentFailedNotification(
                adminId,
                jobInstance.SeniorId,
                jobInstance.OrderId,
                jobInstance.Id,
                culture: "en"
                );

            await _notificationService.StoreAndNotifyAsync(notification);

            var customer = await _customerRepo.GetByIdAsync(jobInstance.CustomerId);
            var customerCulture = customer.Contact.LanguageCode ?? "en";

            // notify customer
            var customerNotification = _notificationFactory.CreatePaymentFailedNotification(
               jobInstance.CustomerId,
               jobInstance.SeniorId,
               jobInstance.OrderId,
               jobInstance.Id,
               culture: customerCulture
               );

            await _notificationService.SendPushNotificationAsync(jobInstance.CustomerId, customerNotification);


            var studentCulture = jobInstance?.ScheduleAssignment?.Student.Contact.LanguageCode ?? "en";

            // notify student
            var studentId = jobInstance.ScheduleAssignment.StudentId;
            var studentNotification = _notificationFactory.JobCancelledNotification(
                studentId,
                jobInstance,
                culture: studentCulture

                );

            await _notificationService.SendPushNotificationAsync(studentId, studentNotification);
        }
        catch (Exception)
        {


        }
    }

    private async Task HandlePaymentSuccess(JobInstance jobInstance, PaymentProfile paymentProfile)
    {
        try
        {
            var customer = await _customerRepo.LoadCustomerWithIncludes(jobInstance.CustomerId, new CustomerIncludeOptions
            {
                Contact = true
            });

            if (customer == null)
            {
                _logger.LogWarning("❌ [Failed] get customer in payment service Customer: {customerId}", jobInstance.CustomerId);
            }

            var invoice = await _minimaxService.ProcessIssuedInvoice(jobInstance, customer!.Contact, paymentProfile!);

            var customerEmail = customer.Contact.Email;
            var culture = customer.Contact.LanguageCode;

            await EmailInvoiceToCustomer(invoice!, customerEmail!, culture);

            await PaymentSuccessNotifyCustomer(jobInstance, culture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [Failed] [HandlePaymentSuccess]");

        }
    }

    private async Task EmailInvoiceToCustomer(MinimaxIssuedInvoice invoice, string customerEmail, string culture)
    {
        try
        {

            _logger.LogInformation("📤 [Sending e-Invoice] ...");

            var attachment = await _minimaxService.GenerateInvoicePdf(
                    (int)invoice!.IssuedInvoiceId!,
                     invoice.RowVersion!
                     );

            var attachmentData = new Dictionary<string, string>
                {
                    { attachment!.AttachmentFileName, attachment.AttachmentData }
                };

            var subject = _loc.GetString("Emails.Invoice.Subject", culture);
            var body = _loc.GetString("Emails.Invoice.Body", culture);

            var success = await _mailgunService.SendEmailAsync(
                  to: customerEmail,
                  subject: subject,
                  htmlBody: body,
                  attachments: attachmentData
              );



            // Create HEmail record
            var emailRecord = new HEmail
            {
                ExternalInvoiceId = (int)invoice.IssuedInvoiceId!,
                Type = EmailType.Invoice,
                Status = success ? EmailStatus.Sent : EmailStatus.Failed,
                OpenedCount = 0,
                AttemptCount = 1,
                LastAttempt = DateTime.UtcNow,
                NextAttempt = success ? null : DateTime.UtcNow.AddMinutes(10),
                ErrorMessage = success ? null : "Failed to send email via Mailgun",

            };

            await _hEmailRepository.AddAsync(emailRecord);

            _logger.LogInformation("✅ [EmailInvoiceToCustomer] Email sent and recorded successfully.");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [Failed] [EmailInvoiceToCustomer]");

        }

    }

    private async Task PaymentSuccessNotifyCustomer(JobInstance jobInstance, string culture)
    {
        var customerNotification = _notificationFactory.CreatePaymentSuccessNotification(
             jobInstance.CustomerId,
             jobInstance.SeniorId,
             jobInstance.OrderId,
             jobInstance.Id,
             culture: culture
             );

        await _notificationService.SendPushNotificationAsync(jobInstance.CustomerId, customerNotification);
    }

}