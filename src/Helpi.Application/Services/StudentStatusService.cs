using System.Data;
using System.Text.Json;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class StudentStatusService
{
    private readonly IStudentRepository _studentRepo;

    private readonly INotificationService _notificationService;
    private readonly INotificationFactory _notificationFactory;
    private readonly StudentsService _studentService;

    private readonly ILogger<OrderStatusMaintenanceService> _logger;

    private readonly IReassignmentService _reassignmentService;
    private readonly IEventMediator _mediator;

    private readonly IContractEvaluationService _contractEvaluator;

    private readonly IMailgunService _mailgunService;
    private readonly ILocalizationService _loc;

    public StudentStatusService(
        IStudentRepository studentRepo,
      StudentsService studentService,
      INotificationService notificationService,
INotificationFactory notificationFactory,
     IReassignmentService reassignmentService,
IEventMediator mediator,
        ILogger<OrderStatusMaintenanceService> logger,
              IContractEvaluationService contractEvaluator,
              IMailgunService mailgunService,
ILocalizationService loc
    )
    {
        _studentRepo = studentRepo;
        _studentService = studentService;
        _notificationService = notificationService;
        _notificationFactory = notificationFactory;
        _logger = logger;
        _reassignmentService = reassignmentService;
        _mediator = mediator;
        _contractEvaluator = contractEvaluator;
        _mailgunService = mailgunService;
        _loc = loc;
    }

    public async Task ProcessStudentContracts()
    {
        _logger.LogInformation("🔍 Starting ProcessStudentStatuses");

        var students = await _studentRepo.LoadStudentsWithIncludes(null,
                        new StudentIncludeOptions
                        {
                            ContactInfo = true,
                            Contracts = true
                        },
                        excludeStatus: [StudentStatus.Deleted],
                        asNoTracking: false
                        );



        foreach (var student in students)
        {
            await ProcessStudentStatus(student);
        }


    }

    /// <summary>
    /// student must have included Include(contracts)
    /// </summary>
    /// <param name="student"></param>
    /// <returns></returns>
    public async Task ProcessStudentStatus(Student student)
    {
        _logger.LogInformation("📅 Processing Student: {StudentId}", student.UserId);

        var eval = _contractEvaluator.Evaluate(student);

        // Update DaysToContractExpire (safe: set null where appropriate)
        UpdateDaysToContractExpire(student, eval.ActiveContract);
        await _studentRepo.UpdateAsync(student);

        // CASE A: Active contract exists
        if (eval.ActiveContract != null)
        {
            await HandleActiveContract(student, eval);
            return;
        }

        // CASE B: no contract at all
        if (student.Contracts.Any() == false)
        {
            await HandleNoContractExists(student, eval);
            return;
        }

        // CASE C: Truly expired or no contract
        await HandleTrulyExpired(student, eval);

    }

    private async Task HandleActiveContract(Student student, ContractEvaluationResult eval)
    {
        var active = eval.ActiveContract!;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysUntilExpiry = active.ExpirationDate.DayNumber - today.DayNumber;

        var aboutToExpire = daysUntilExpiry <= 5 && daysUntilExpiry >= 0;
        var notifiedBoutToExpire = student.Status == StudentStatus.ContractAboutToExpire;

        if (aboutToExpire && !notifiedBoutToExpire)
        {
            if (eval.NextContract != null && eval.HasGap == false)
            {
                await HandleSmoothTransition(student, eval);
                return;
            }
            else
            {
                await HandleContractRenewalReminder(student, active);
                return;
            }

        }

        // Ensure student is Active if they have an active contract
        if (student.Status != StudentStatus.Active)
        {
            student.Status = StudentStatus.Active;
            await _studentRepo.UpdateAsync(student);

            var notification = _notificationFactory.StudentContractAdded(
                student.UserId,
                active.Id,
                culture: student.Contact.LanguageCode ?? "en");

            await _notificationService.SendNotificationAsync(student.UserId, notification);

            await _mediator.Publish(new ReinitiateAllFailedMatchesEvent());
        }
    }

    private async Task HandleSmoothTransition(Student student, ContractEvaluationResult eval)
    {
        _logger.LogInformation("Student {StudentId} has immediate next contract; skipping expiry flows", student.UserId);

        // If not active, set active-like status (we keep Active)
        if (student.Status != StudentStatus.Active)
        {
            student.Status = StudentStatus.Active;
            await _studentRepo.UpdateAsync(student);
        }
    }


    private async Task HandleNoContractExists(Student student, ContractEvaluationResult eval)
    {
        _logger.LogInformation("Student {StudentId} NO contract extists", student.UserId);

        if (student.Status == StudentStatus.InActive) return;

        student.Status = StudentStatus.InActive;
        await _studentRepo.UpdateAsync(student);

    }

    private async Task HandleTrulyExpired(Student student, ContractEvaluationResult eval)
    {
        // eval.ActiveContract == null
        // eval.NextContract == null || eval.HasGap == true
        var daysSinceExpiry = eval.DaysSinceExpiry ?? 0;

        _logger.LogInformation("Contract expired {DaysSinceExpiry} days ago for student {StudentId}",
            daysSinceExpiry, student.UserId);


        var activeStatuses = new[]
        {
            StudentStatus.InActive,
            StudentStatus.Active,
            StudentStatus.ContractAboutToExpire
        };

        var expiredButNotMarked = activeStatuses.Contains(student.Status);

        if (expiredButNotMarked)
        {
            student.Status = StudentStatus.Expired;
            await _reassignmentService.ReassignExpiredContractJobs(student.UserId);
            await _studentRepo.UpdateAsync(student);
            // send initial expired notification (day 0)
            if (eval.DaysSinceExpiry == 0 || eval.DaysSinceExpiry == null)
            {
                // consider this moment the immediate expiration notice
                var last = student.Contracts.OrderByDescending(c => c.ExpirationDate).FirstOrDefault();
                if (last != null)
                    await SendContractExpiredNotification(student, last);
            }
            return;
        }

        // If it was already expired but it's the first day after expiry -> reassign & notify
        // if (daysSinceExpiry == 1)
        // {
        //     await _reassignmentService.ReassignExpiredContractJobs(student.UserId);
        //     var last = student.Contracts.OrderByDescending(c => c.ExpirationDate).FirstOrDefault();
        //     if (last != null) await SendContractExpiredNotification(student, last);
        // }

        // lifecycle actions at 90 and 180 days
        const int ThreeMonthsInDays = 90;
        const int SixMonthsInDays = 180;
        const int SevenDaysBeforeSixMonths = SixMonthsInDays - 7; // 173

        switch (daysSinceExpiry)
        {
            case SevenDaysBeforeSixMonths:
                var lc = student.Contracts.OrderByDescending(c => c.ExpirationDate).FirstOrDefault();
                if (lc != null) await SendFinalAccountDeletionWarningEmail(student, lc);
                break;
            case ThreeMonthsInDays:
                await _studentService.SoftDeleteStudent(student.UserId);
                break;
            case SixMonthsInDays:
                await _studentService.PermanentlyDeleteStudent(student.UserId);
                break;
            default:
                if (daysSinceExpiry < ThreeMonthsInDays)
                {
                    _logger.LogInformation("Student {StudentId} expired {Days} days ago - within grace period",
                        student.UserId, daysSinceExpiry);
                }
                else if (daysSinceExpiry < SixMonthsInDays)
                {
                    _logger.LogInformation("Student {StudentId} soft-deleted, {Days} days until permanent deletion",
                        student.UserId, SixMonthsInDays - daysSinceExpiry);
                }
                break;
        }
    }

    private async Task HandleContractRenewalReminder(Student student, StudentContract contract)
    {
        _logger.LogInformation("🔔 Sending contract renewal reminder notification to student {StudentId}", student.UserId);

        try
        {
            student.Status = StudentStatus.ContractAboutToExpire;
            await _studentRepo.UpdateAsync(student);

            var notification = _notificationFactory.StudentContractAboutToExpire(student.UserId,
                contract.Id,
                culture: student.Contact.LanguageCode ?? "en");

            await _notificationService.SendNotificationAsync(student.UserId, notification);

            _logger.LogInformation("✅ Contract renewal reminder notification sent to student {StudentId}", student.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send contract renewal reminder notification to student {StudentId}", student.UserId);
        }
    }

    private async Task SendFinalAccountDeletionWarningEmail(Student student, StudentContract contract)
    {
        _logger.LogInformation("📧 Sending final warning email to student {StudentId}", student.UserId);

        try
        {

            var email = student.Contact.Email;
            var culture = student.Contact.LanguageCode;
            var daysToAccountDelete = 7;

            var subject = _loc.GetString("Emails.Student.FinalAccountDeletionWarning.Subject", culture);
            var body = _loc.GetString(
                "Emails.Student.FinalAccountDeletionWarning.Body",
                culture,
                student.Contact.FullName,
                contract.ExpirationDate.ToString("yyyy-MM-dd"),
                daysToAccountDelete
            );

            await _mailgunService.SendEmailAsync(email!, subject, body);

            // Implement email send via email service
            _logger.LogInformation("✅ Final warning email sent to student {StudentId}", student.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send final warning email to student {StudentId}", student.UserId);
        }
    }

    private async Task SendContractExpiredNotification(Student student, StudentContract contract)
    {
        _logger.LogInformation("🔔 Sending contract expired notification to student {StudentId}", student.UserId);

        try
        {

            var notification = _notificationFactory.StudentContractExpired(student.UserId,
                contract.Id,
                culture: student.Contact.LanguageCode ?? "en");

            await _notificationService.SendNotificationAsync(student.UserId, notification);

            _logger.LogInformation("✅ Contract expired notification sent to {StudentId}", student.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send contract expired notification to student {StudentId}", student.UserId);
        }
    }

    private void UpdateDaysToContractExpire(Student student, StudentContract? contract)
    {
        if (contract == null)
        {
            student.DaysToContractExpire = null;
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        student.DaysToContractExpire = contract.ExpirationDate.DayNumber - today.DayNumber;
    }
}

