using System.Data;
using System.Text.Json;
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
    private readonly StudentsService _studentService;

    private readonly ILogger<CompletionStatusService> _logger;

    private readonly IReassignmentService _reassignmentService;
    private readonly IEventMediator _mediator;

    public StudentStatusService(
        IStudentRepository studentRepo,
      StudentsService studentService,
      INotificationService notificationService,
     IReassignmentService reassignmentService,
IEventMediator mediator,
        ILogger<CompletionStatusService> logger
    )
    {
        _studentRepo = studentRepo;
        _studentService = studentService;
        _notificationService = notificationService;
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


        var activeContract = student.Contracts
            .Where(c => c.Status == ContractStatus.Active)
            .OrderByDescending(c => c.ExpirationDate)
            .FirstOrDefault();

        if (activeContract == null)
        {
            _logger.LogWarning("No active contract found for student {StudentId}", student.UserId);
            // eg all expired || no contract
            await HandleStudentWithoutActiveContract(student);
            return;
        }

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
            if (student.Status != StudentStatus.Verified)
            {
                student.Status = StudentStatus.Verified;
                await _studentRepo.UpdateAsync(student);


                var notification = new HNotification
                {
                    RecieverUserId = student.UserId,
                    Title = "Contract valid",
                    Body = "Contract valid",
                    Type = NotificationType.contractActive,
                    Payload = JsonSerializer.Serialize(new
                    {
                        RecieverUserId = student.UserId,
                        ContractId = activeContract.Id,
                    })
                };

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

        if (lastContract != null)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var daysSinceExpiry = today.DayNumber - lastContract.ExpirationDate.DayNumber;

            await HandleExpiredContract(student, lastContract, daysSinceExpiry);
        }
        else
        {
            _logger.LogWarning("Student {StudentId} has no contracts at all", student.UserId);
        }
    }

    private async Task HandleExpiredContract(Student student, StudentContract contract, int daysSinceExpiry)
    {
        const int ThreeMonthsInDays = 90;
        const int SixMonthsInDays = 180;
        const int SevenDaysBeforeSixMonths = SixMonthsInDays - 7; // 173 days

        _logger.LogInformation("Contract expired {DaysSinceExpiry} days ago for student {StudentId}",
            daysSinceExpiry, student.UserId);


        if (student.Status == StudentStatus.Verified)
        {
            student.Status = StudentStatus.UnVerified;
            await _studentRepo.UpdateAsync(student);
        }


        // Reassign jobs immediately when contract expires
        if (daysSinceExpiry == 1)
        {
            try
            {
                await _reassignmentService.ReassignExpiredContractJobs(student.UserId);
            }
            catch
            {
                //
            }
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

            student.Status = StudentStatus.ContractRenewalNeeded;
            await _studentRepo.UpdateAsync(student);

            var notification = new HNotification
            {
                RecieverUserId = student.UserId,
                Title = "Renew contract",
                Body = "Contract needs to be renewed",
                Type = NotificationType.ContractRenewalRequired,
                Payload = JsonSerializer.Serialize(new
                {
                    RecieverUserId = student.UserId,
                    ContractId = contract.Id,
                })
            };

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


}

