using System.Text.Json;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;


public class JobInstanceMatchingService : IJobInstanceMatchingService
{
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IReassignmentRecordRepository _reassignmentRecordRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IJobRequestRepository _jobRequestRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IOrderRepository _orderRepository;
    private readonly ISeniorRepository _seniorRepository;
    private readonly ILogger<JobInstanceMatchingService> _logger;
    private readonly IMatchingBackgroundJobs _matchingBackgroundJobs;

    // Configuration parameters
    private readonly int _maxConcurrentNotifications = 1;
    private readonly int _jobRequestExpirationMinutes = 10;
    private readonly int _retryIntervalMinutes = 10;
    private readonly int _maxMatchingAttempts = 300000;

    public JobInstanceMatchingService(
        IJobInstanceRepository jobInstanceRepository,
        IReassignmentRecordRepository reassignmentRecordRepository,
        IStudentRepository studentRepository,
        IJobRequestRepository jobRequestRepository,
        INotificationService notificationService,
INotificationFactory notificationFactory,
        IOrderRepository orderRepository,
        ISeniorRepository seniorRepository,
        ILogger<JobInstanceMatchingService> logger,
        IMatchingBackgroundJobs matchingBackgroundJobs)
    {
        _jobInstanceRepository = jobInstanceRepository;
        _reassignmentRecordRepository = reassignmentRecordRepository;
        _studentRepository = studentRepository;
        _jobRequestRepository = jobRequestRepository;
        _notificationService = notificationService;
        _notificationFactory = notificationFactory;
        _orderRepository = orderRepository;
        _seniorRepository = seniorRepository;
        _logger = logger;
        _matchingBackgroundJobs = matchingBackgroundJobs;
    }

    public async Task StartJobInstanceMatchingAsync(int jobInstanceId, int reassignmentRecordId)
    {
        _logger.LogInformation("🚀 Starting job instance matching for instance {JobInstanceId}, reassignment record {RecordId}",
            jobInstanceId, reassignmentRecordId);

        // Schedule the first matching attempt
        ScheduleNextMatchingAttempt(jobInstanceId, reassignmentRecordId, DateTime.UtcNow.AddSeconds(5));
    }

    public async Task ProcessJobInstanceMatchingAsync(int jobInstanceId, int reassignmentRecordId)
    {
        try
        {
            _logger.LogInformation("🔍 Processing job instance matching for instance {JobInstanceId}, reassignment record {RecordId}",
                jobInstanceId, reassignmentRecordId);

            // Get the job instance and reassignment record
            var jobInstance = await _jobInstanceRepository.LoadJobInstanceWithIncludes(jobInstanceId, new JobInstanceIncludeOptions
            {
                Order = true,
                OrderSchedule = true,
            });


            var reassignmentRecord = await _reassignmentRecordRepository.GetByIdAsync(
                                    reassignmentRecordId,
                                    new ReassignmentIncludeOptions { },
                                    asNoTracking: true
                                    );

            if (jobInstance == null || reassignmentRecord == null)
            {
                _logger.LogWarning("⚠️ Job instance {JobInstanceId} or reassignment record {RecordId} not found",
                    jobInstanceId, reassignmentRecordId);
                return;
            }

            // Check if reassignment is still in progress
            if (reassignmentRecord.Status != ReassignmentStatus.InProgress)
            {
                _logger.LogInformation("📋 Reassignment record {RecordId} is no longer in progress (status: {Status}), stopping matching",
                    reassignmentRecordId, reassignmentRecord.Status);
                return;
            }

            // Check if we've exceeded max attempts
            if (reassignmentRecord.AttemptCount >= _maxMatchingAttempts)
            {
                _logger.LogWarning("⚠️ Max matching attempts reached for reassignment record {RecordId}", reassignmentRecordId);
                await HandleMaxAttemptsReached(reassignmentRecord);
                return;
            }

            // Get the order and senior information
            var order = jobInstance.Order;
            var senior = jobInstance.Senior;

            // Get students who have already been notified for this reassignment
            var notifiedStudentIds = await _jobRequestRepository.NotifiedStudentIdsForReassignment(reassignmentRecordId);

            // Find qualified students for this specific job instance
            var qualifiedStudents = await FindQualifiedStudentsForInstance(jobInstance,
                reassignmentRecord,
                order.OrderServices.ToList(),
                notifiedStudentIds,
                reassignmentRecord.PreferredStudentId
                );

            if (!qualifiedStudents.Any())
            {
                if (notifiedStudentIds.Any())
                {
                    _logger.LogInformation("📝 All qualified students already notified for reassignment record {RecordId}", reassignmentRecordId);
                    await HandleAllStudentsNotified(jobInstance, reassignmentRecord, notifiedStudentIds);
                    return;
                }
                else
                {
                    _logger.LogWarning("⚠️ No qualified students found for job instance {JobInstanceId}", jobInstanceId);
                    await HandleNoQualifiedStudents(jobInstance, reassignmentRecord);
                    return;
                }
            }



            // Filter out already notified students
            var unnotifiedStudents = qualifiedStudents;



            // Take the top N students based on prioritization
            var studentsToNotify = unnotifiedStudents
                .Take(_maxConcurrentNotifications)
                .ToList();

            // Send job requests to each student
            var anyNotificationSent = false;
            foreach (var student in studentsToNotify)
            {
                var notificationSent = await TrySendJobRequest(student, jobInstance, reassignmentRecord);
                if (notificationSent) anyNotificationSent = true;
            }

            // Update attempt count
            reassignmentRecord.AttemptCount++;
            reassignmentRecord.LastAttemptAt = DateTime.UtcNow;
            await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

            // Schedule next attempt if we haven't reached max attempts
            if (reassignmentRecord.AttemptCount < _maxMatchingAttempts && anyNotificationSent)
            {
                ScheduleNextMatchingAttempt(jobInstanceId, reassignmentRecordId,
                    DateTime.UtcNow.AddMinutes(_retryIntervalMinutes));
            }
            else if (!anyNotificationSent)
            {
                _logger.LogWarning("⚠️ No notifications sent for reassignment record {RecordId}, stopping matching", reassignmentRecordId);
                await HandleMatchingCancel(reassignmentRecord, "No notifications could be sent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing job instance matching for instance {JobInstanceId}, reassignment record {RecordId}",
                jobInstanceId, reassignmentRecordId);
        }
    }

    private async Task<List<Student>> FindQualifiedStudentsForInstance(
        JobInstance jobInstance,
         ReassignmentRecord reassignmentRecord,
          List<OrderService> orderServices,
         List<int> notifiedStudentIds,
         int? preferedStudentId
          )
    {
        // Get the order to find required services
        var order = await _orderRepository.LoadOrderWithIncludes(jobInstance.OrderId,
         new OrderIncludeOptions { Senior = true }
        );

        if (order == null)
        {
            throw new ArgumentException($"Order {jobInstance.OrderId} not found");
        }

        var serviceIds = orderServices.Select(s => s.ServiceId).ToList();


        // Also exclude the original student if this is a reassignment

        if (reassignmentRecord.OriginalStudentId.HasValue)
        {
            var originalStudentId = reassignmentRecord.OriginalStudentId.Value;

            // mark original student as notified , unless if he is mentioned as prefered
            if (preferedStudentId != null)
            {
                if (preferedStudentId != originalStudentId)
                {
                    notifiedStudentIds.Add(reassignmentRecord.OriginalStudentId.Value);
                }
            }

        }

        // Find students qualified for the services and available at the specific time
        var availableStudents = await _studentRepository.FindEligibleStudentsForInstance(
            jobInstance.ScheduledDate,
            jobInstance.StartTime,
            jobInstance.EndTime,
            order.Senior.Contact.CityId,
            serviceIds,
            notifiedStudentIds,
             reassignmentRecord?.PreferredStudentId
        );



        // Prioritize students
        return availableStudents;
    }



    private async Task<bool> TrySendJobRequest(
            Student student,
            JobInstance jobInstance,
            ReassignmentRecord reassignmentRecord
    )
    {
        try
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_jobRequestExpirationMinutes);

            // Create JobRequest entity
            var jobRequest = new JobRequest
            {
                OrderScheduleId = jobInstance.OrderScheduleId,
                JobInstanceId = jobInstance.Id,
                OrderId = jobInstance.OrderId,
                SeniorId = jobInstance.SeniorId,
                StudentId = student.UserId,
                Status = JobRequestStatus.Pending,
                ExpiresAt = expiresAt,
                PriorityLevel = 2, // Higher priority for reassignments
                IsReassignment = true,
                ReassignmentRecordId = reassignmentRecord.Id,
                ReassignmentType = reassignmentRecord.ReassignmentType,
                ReassignJobInstanceId = jobInstance.Id
            };

            // _jobInstanceRepository.Detach(jobInstance);
            // _reassignmentRecordRepository.Detach(reassignmentRecord);
            await _jobRequestRepository.AddAsync(jobRequest);

            // Send notification
            var jobRequestNotification = _notificationFactory.JobRequestNotification(
                                student.UserId,
                                jobInstance.OrderSchedule!,
                                reassignmentRecord,
                                student.Contact.LanguageCode ?? "en"
                                );


            bool notificationSent = await _notificationService.SendNotificationAsync(
                student.UserId, jobRequestNotification);

            if (notificationSent)
            {
                _logger.LogInformation("✅ Job request sent to student {StudentId} for instance {InstanceId}",
                    student.UserId, jobInstance.Id);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to send notification to student {StudentId} for instance {InstanceId}",
                    student.UserId, jobInstance.Id);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending job request to student {StudentId} for instance {InstanceId}",
                student.UserId, jobInstance.Id);
            return false;
        }
    }

    private void ScheduleNextMatchingAttempt(int jobInstanceId, int reassignmentRecordId, DateTime executionTime)
    {
        _matchingBackgroundJobs.ScheduleJobInstanceMatching(jobInstanceId, reassignmentRecordId, executionTime);
        _logger.LogInformation("⏰ Scheduled next matching attempt for instance {JobInstanceId}, reassignment record {RecordId} at {Time}",
            jobInstanceId, reassignmentRecordId, executionTime);
    }

    private async Task HandleMaxAttemptsReached(ReassignmentRecord reassignmentRecord)
    {
        reassignmentRecord.Status = ReassignmentStatus.MaxAttemptsReached;
        reassignmentRecord.Reason += "; Max matching attempts reached";
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin

    }

    private async Task HandleNoQualifiedStudents(JobInstance jobInstance, ReassignmentRecord reassignmentRecord)
    {
        reassignmentRecord.Status = ReassignmentStatus.NoEligibleStudents;
        reassignmentRecord.AllowAutoScheduling = false;
        reassignmentRecord.Reason += "; No qualified students found";
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
        var adminId = await GetAdminId();
        var notification = _notificationFactory.NoEligibleStudentsNotification(
                   adminId,
                   order: reassignmentRecord.Order,
                   schedule: reassignmentRecord.OrderSchedule,
                   reassignment: reassignmentRecord
                   );

        await _notificationService.StoreAndNotifyAsync(notification);

    }

    private async Task HandleAllStudentsNotified(JobInstance jobInstance, ReassignmentRecord reassignmentRecord, List<int> notifiedStudentIds)
    {
        reassignmentRecord.Status = ReassignmentStatus.AllEligableStudentNotified;
        reassignmentRecord.Reason += $"; All {notifiedStudentIds.Count} qualified students notified, none accepted";
        reassignmentRecord.AllowAutoScheduling = false;
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
        var adminId = await GetAdminId();
        var notification = _notificationFactory.AllEligibleStudentsNotified(
                   adminId,
                   order: reassignmentRecord.Order,
                   schedule: reassignmentRecord.OrderSchedule,
                   reassignment: reassignmentRecord
                   );

        await _notificationService.StoreAndNotifyAsync(notification);

        // Also update the job instance status if needed
        jobInstance.NeedsSubstitute = false;
        await _jobInstanceRepository.UpdateAsync(jobInstance);
    }

    private async Task HandleMatchingCancel(ReassignmentRecord reassignmentRecord, string reason)
    {
        reassignmentRecord.Status = ReassignmentStatus.Cancelled;
        reassignmentRecord.Reason += $"; {reason}";
        reassignmentRecord.AllowAutoScheduling = false;
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
    }


    private Task<int> GetAdminId() => Task.FromResult(1);
}