
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
    private readonly IUserRepository _userRepository;

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
ILocalizationService loc,
IUserRepository userRepository
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
        _userRepository = userRepository;
    }


    public async Task ProcessPaymentAsync(int jobInstanceId)
    {
        var jobInstance = await _jobInstanceRepo.LoadJobInstanceWithIncludes(jobInstanceId, new SessionIncludeOptions
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

        await HandlePaymentSuccess(jobInstance, paymentProfile!, transaction);


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
            var adminIds = await _userRepository.GetAdminIdsAsync();
            // notify admins
            await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                adminId => _notificationFactory.CreatePaymentFailedNotification(
                    adminId,
                    jobInstance.SeniorId,
                    jobInstance.OrderId,
                    jobInstance.Id,
                    culture: "hr"
                    ));

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

            await _notificationService.SendNotificationAsync(jobInstance.CustomerId, customerNotification);


            var studentCulture = jobInstance?.ScheduleAssignment?.Student.Contact.LanguageCode ?? "en";

            // notify student
            var studentId = jobInstance.ScheduleAssignment.StudentId;
            var studentNotification = _notificationFactory.JobCancelledNotification(
                studentId,
                jobInstance,
                culture: studentCulture

                );

            await _notificationService.SendNotificationAsync(studentId, studentNotification);
        }
        catch (Exception)
        {


        }
    }

    private async Task HandlePaymentSuccess(JobInstance jobInstance, PaymentProfile paymentProfile, PaymentTransaction transaction)
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
                transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
                await _transactionRepository.UpdateAsync(transaction);
                return;
            }

            var invoice = await _minimaxService.ProcessIssuedInvoice(jobInstance, customer!.Contact, paymentProfile!);

            if (invoice == null)
            {
                _logger.LogWarning("❌ [Failed] Minimax invoice creation for transaction {TransactionId}", transaction.Id);
                transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
                await _transactionRepository.UpdateAsync(transaction);
                return;
            }

            transaction.InvoiceCreationStatus = InvoiceCreationStatus.Created;
            transaction.MinimaxInvoiceId = invoice.IssuedInvoiceId;
            await _transactionRepository.UpdateAsync(transaction);

            var customerEmail = customer.Contact.Email;
            var culture = customer.Contact.LanguageCode;

            await EmailInvoiceToCustomer(invoice!, customerEmail!, culture);

            await PaymentSuccessNotifyCustomer(jobInstance, culture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [Failed] [HandlePaymentSuccess] for transaction {TransactionId}", transaction.Id);
            transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
            await _transactionRepository.UpdateAsync(transaction);
        }
    }

    /// <summary>
    /// Retries Minimax invoice creation for a paid transaction where invoice failed.
    /// Safe to call multiple times — checks for existing invoice before creating.
    /// </summary>
    public async Task<bool> RetryInvoiceAsync(int transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            _logger.LogWarning("❌ Transaction {Id} not found for invoice retry", transactionId);
            return false;
        }

        if (transaction.Status != PaymentStatus.Paid)
        {
            _logger.LogWarning("⚠️ Transaction {Id} status is {Status}, not Paid — skipping", transactionId, transaction.Status);
            return false;
        }

        if (transaction.InvoiceCreationStatus == InvoiceCreationStatus.Created)
        {
            _logger.LogInformation("✅ Transaction {Id} already has invoice (MinimaxId={MinimaxId}) — skipping",
                transactionId, transaction.MinimaxInvoiceId);
            return true;
        }

        transaction.InvoiceRetryCount++;
        await _transactionRepository.UpdateAsync(transaction);

        var paymentProfile = await _paymentProfileRepo.GetStipePaymentByUserIdAsync(transaction.CustomerId);
        if (paymentProfile == null)
        {
            _logger.LogWarning("❌ No PaymentProfile for customer {CustomerId}", transaction.CustomerId);
            transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
            await _transactionRepository.UpdateAsync(transaction);
            return false;
        }

        var jobInstance = await _jobInstanceRepo.LoadJobInstanceWithIncludes(transaction.JobInstanceId, new SessionIncludeOptions
        {
            Order = true,
            OrderPaymentMethod = true,
            Assignment = true,
            AssignmentStudent = true,
        });

        if (jobInstance == null)
        {
            _logger.LogWarning("❌ JobInstance {Id} not found for invoice retry", transaction.JobInstanceId);
            transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
            await _transactionRepository.UpdateAsync(transaction);
            return false;
        }

        var customer = await _customerRepo.LoadCustomerWithIncludes(transaction.CustomerId, new CustomerIncludeOptions
        {
            Contact = true
        });

        if (customer == null)
        {
            _logger.LogWarning("❌ Customer {Id} not found for invoice retry", transaction.CustomerId);
            transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
            await _transactionRepository.UpdateAsync(transaction);
            return false;
        }

        var invoice = await _minimaxService.ProcessIssuedInvoice(jobInstance, customer.Contact, paymentProfile);

        if (invoice == null)
        {
            _logger.LogWarning("❌ Invoice retry failed for transaction {Id} (attempt {Count})", transactionId, transaction.InvoiceRetryCount);
            transaction.InvoiceCreationStatus = InvoiceCreationStatus.Failed;
            await _transactionRepository.UpdateAsync(transaction);
            return false;
        }

        transaction.InvoiceCreationStatus = InvoiceCreationStatus.Created;
        transaction.MinimaxInvoiceId = invoice.IssuedInvoiceId;
        await _transactionRepository.UpdateAsync(transaction);

        _logger.LogInformation("✅ Invoice retry succeeded for transaction {Id} — MinimaxId={MinimaxId}",
            transactionId, invoice.IssuedInvoiceId);

        var customerEmail = customer.Contact.Email;
        var culture = customer.Contact.LanguageCode;
        await EmailInvoiceToCustomer(invoice, customerEmail!, culture);

        return true;
    }

    /// <summary>
    /// Called by Hangfire — retries all failed invoices.
    /// </summary>
    public async Task RetryFailedInvoicesAsync()
    {
        var failedTransactions = await _transactionRepository.GetFailedInvoiceTransactionsAsync();
        var count = failedTransactions.Count();

        if (count == 0)
        {
            _logger.LogInformation("✅ No failed invoices to retry.");
            return;
        }

        _logger.LogInformation("🔁 Retrying {Count} failed invoice(s)...", count);

        foreach (var transaction in failedTransactions)
        {
            await RetryInvoiceAsync(transaction.Id);
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

        await _notificationService.SendNotificationAsync(jobInstance.CustomerId, customerNotification);

        // Notify admins about payment success
        var adminIds = await _userRepository.GetAdminIdsAsync();
        await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
            adminId => _notificationFactory.CreatePaymentSuccessNotification(
                adminId,
                jobInstance.SeniorId,
                jobInstance.OrderId,
                jobInstance.Id,
                culture: "hr"
            ));
    }

}