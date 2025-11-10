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

    public StudentStatusService(
        IStudentRepository studentRepo,
      StudentsService studentService,
      INotificationService notificationService,
INotificationFactory notificationFactory,
     IReassignmentService reassignmentService,
IEventMediator mediator,
        ILogger<OrderStatusMaintenanceService> logger
    )
    {
        _studentRepo = studentRepo;
        _studentService = studentService;
        _notificationService = notificationService;
        _notificationFactory = notificationFactory;
        _logger = logger;
        _reassignmentService = reassignmentService;
        _mediator = mediator;
    }

    public async Task ProcessStudentContracts()
    {

        /// todo : concider puting a check to see if its 
        _logger.LogInformation("🔍 Starting ProcessStudentStatuses");



        var students = await _studentRepo.LoadStudentsWithIncludes(null, new StudentIncludeOptions
        {
            Contracts = true
        }
        ,
         excludeStatus: [StudentStatus.Deleted]);



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


        var activeContract = student.ActiveContract;



        if (activeContract == null)
        {
            _logger.LogWarning("No active contract found for student {StudentId}", student.UserId);
            // eg all expired || no contract
            await HandleStudentWithoutActiveContract(student);
            return;
        }

        UpdateDaysToContractExpire(student, activeContract);
        await _studentRepo.UpdateAsync(student);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysUntilExpiry = activeContract.ExpirationDate.DayNumber - today.DayNumber;
        var daysSinceExpiry = today.DayNumber - activeContract.ExpirationDate.DayNumber;

        _logger.LogInformation("Contract expires on {ExpirationDate}, Days until/since expiry: {Days}",
            activeContract.ExpirationDate, daysUntilExpiry);

        // Check if contract is expiring in 5 days
        if (daysUntilExpiry == 5 && daysUntilExpiry > 0)
        {
            await HandleContractRenewalNeeded(student, activeContract);
        }
        // Check if contract expired and handle deletion timeline
        else if (daysSinceExpiry > 0)
        {
            await HandleExpiredContract(student, activeContract, daysSinceExpiry);
        }
        else
        {
            if (student.Status != StudentStatus.Active)
            {
                student.Status = StudentStatus.Active;
                await _studentRepo.UpdateAsync(student);

                var notification = _notificationFactory.StudentContractAdded(
                     student.UserId,
                     activeContract.Id,
                     culture: student.Contact.LanguageCode ?? "en"
                     );

                await _notificationService.SendPushNotificationAsync(student.UserId, notification);

                await _mediator.Publish(new ReinitiateAllFailedMatchesEvent());
            }
        }
    }

    private async Task HandleStudentWithoutActiveContract(Student student)
    {
        // Find the most recent expired contract to determine timeline
        var lastContract = student.Contracts
            .OrderByDescending(c => c.ExpirationDate)
            .FirstOrDefault();

        UpdateDaysToContractExpire(student, lastContract);
        await _studentRepo.UpdateAsync(student);

        if (lastContract != null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var daysSinceExpiry = today.DayNumber - lastContract.ExpirationDate.DayNumber;

            await HandleExpiredContract(student, lastContract, daysSinceExpiry);
        }
        else
        {
            _logger.LogWarning("Student {StudentId} has no contracts at all", student.UserId);

            if (student.Status != StudentStatus.InActive)
            {
                student.Status = StudentStatus.InActive;
                await _studentRepo.UpdateAsync(student);
            }

        }
    }

    private async Task HandleExpiredContract(Student student, StudentContract contract, int daysSinceExpiry)
    {
        const int ThreeMonthsInDays = 90;
        const int SixMonthsInDays = 180;
        const int SevenDaysBeforeSixMonths = SixMonthsInDays - 7; // 173 days

        _logger.LogInformation("Contract expired {DaysSinceExpiry} days ago for student {StudentId}",
            daysSinceExpiry, student.UserId);

        bool expiredButNotMarked = student.Status == StudentStatus.Active;


        if (expiredButNotMarked)
        {
            student.Status = StudentStatus.Expired;
            await _reassignmentService.ReassignExpiredContractJobs(student.UserId);
            await _studentRepo.UpdateAsync(student);
            await SendContractENotification(student, contract);
        }
        else if (daysSinceExpiry == 1)
        {
            await _reassignmentService.ReassignExpiredContractJobs(student.UserId);
            await SendContractENotification(student, contract);
        }


        switch (daysSinceExpiry)
        {
            case SevenDaysBeforeSixMonths:
                // Send final warning email 7 days before permanent deletion (173 days after expiry)
                await SendFinalWarningEmail(student, contract);
                break;

            case ThreeMonthsInDays:
                // Delete account but keep ID linked (90 days after expiry)
                await _studentService.SoftDeleteStudent(student.UserId);
                break;

            case SixMonthsInDays:
                // Permanent deletion from database and admin panel (180 days after expiry)
                await _studentService.PermanentlyDeleteStudent(student.UserId);
                break;

            default:
                // Log current status but no action needed
                if (daysSinceExpiry < ThreeMonthsInDays)
                {
                    _logger.LogInformation("Student {StudentId} contract expired {Days} days ago - within grace period",
                        student.UserId, daysSinceExpiry);
                }
                else if (daysSinceExpiry < SixMonthsInDays)
                {
                    _logger.LogInformation("Student {StudentId} account soft-deleted, {Days} days until permanent deletion",
                        student.UserId, SixMonthsInDays - daysSinceExpiry);
                }
                break;
        }
    }

    private async Task HandleContractRenewalNeeded(Student student, StudentContract contract)
    {
        _logger.LogInformation("🔔 Sending contract renewal notification to student {StudentId}", student.UserId);

        try
        {

            student.Status = StudentStatus.ContractAboutToExpire;
            await _studentRepo.UpdateAsync(student);

            var notification = _notificationFactory.StudentContractAboutToExpire(student.UserId,
             contract.Id,
            culture: student.Contact.LanguageCode ?? "en"
            );

            await _notificationService.SendPushNotificationAsync(student.UserId, notification);

            _logger.LogInformation("✅ Contract renewal notification sent to student {StudentId}", student.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send contract renewal notification to student {StudentId}", student.UserId);
        }
    }

    private async Task SendFinalWarningEmail(Student student, StudentContract contract)
    {
        _logger.LogInformation("📧 Sending final warning email via MailerLite to student {StudentId}", student.UserId);

        try
        {
            // Send MailerLite email (implement based on your MailerLite integration)
            // await _emailService.SendMailerLiteEmail(student.Contact.Email, new EmailDto
            // {
            //     Template = "FinalWarningTemplate",
            //     Subject = "Final Notice - Account Deletion in 7 Days",
            //     Data = new Dictionary<string, object>
            //     {
            //         ["StudentName"] = $"{student.Contact.FirstName} {student.Contact.LastName}",
            //         ["StudentNumber"] = student.StudentNumber,
            //         ["ContractExpirationDate"] = contract.ExpirationDate.ToString("yyyy-MM-dd"),
            //         ["DeletionDate"] = contract.ExpirationDate.AddDays(180).ToString("yyyy-MM-dd")
            //     }
            // });

            _logger.LogInformation("✅ Final warning email sent to student {StudentId}", student.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send final warning email to student {StudentId}", student.UserId);
        }
    }

    private async Task SendContractENotification(Student student, StudentContract contract)
    {
        _logger.LogInformation("🔔 Sending contract expired notification to student {StudentId}", student.UserId);

        try
        {

            student.Status = StudentStatus.ContractAboutToExpire;
            await _studentRepo.UpdateAsync(student);

            var notification = _notificationFactory.StudentContractExpired(student.UserId,
            contract.Id,
            culture: student.Contact.LanguageCode ?? "en");

            await _notificationService.SendPushNotificationAsync(student.UserId, notification);

            _logger.LogInformation("✅ Contract expired {StudentId}", student.UserId);
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

        // If already expired, set 0
        // if (today > contract.ExpirationDate)
        // {
        //     student.DaysToContractExpire = 0;
        //     return;
        // }

        // Otherwise, calculate remaining days
        student.DaysToContractExpire = contract.ExpirationDate.DayNumber - today.DayNumber;



    }


}

