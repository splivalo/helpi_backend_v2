using System.Text.Json;
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
                Assignment = true
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
                _logger.LogWarning("⚠️ No qualified students found for job instance {JobInstanceId}", jobInstanceId);
                await HandleNoQualifiedStudents(jobInstance, reassignmentRecord);
                return;
            }



            // Filter out already notified students
            var unnotifiedStudents = qualifiedStudents
                .Where(s => !notifiedStudentIds.Contains(s.UserId))
                .ToList();

            if (!unnotifiedStudents.Any())
            {
                _logger.LogInformation("📝 All qualified students already notified for reassignment record {RecordId}", reassignmentRecordId);
                await HandleAllStudentsNotified(jobInstance, reassignmentRecord, notifiedStudentIds);
                return;
            }

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
                await HandleMatchingFailed(reassignmentRecord, "No notifications could be sent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing job instance matching for instance {JobInstanceId}, reassignment record {RecordId}",
                jobInstanceId, reassignmentRecordId);

            // Update reassignment record with error
            var reassignmentRecord = await _reassignmentRecordRepository.GetByIdAsync(
                        reassignmentRecordId,
                        new ReassignmentIncludeOptions { }
                        );

            if (reassignmentRecord != null)
            {
                reassignmentRecord.Status = ReassignmentStatus.Failed;
                reassignmentRecord.Reason += $"; Error: {ex.Message}";
                await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);
            }
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
         new OrderIncludeOptions { Senior = true });

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
            if (preferedStudentId != originalStudentId)
            {
                notifiedStudentIds.Add(reassignmentRecord.OriginalStudentId.Value);
            }

        }

        // Find students qualified for the services and available at the specific time
        var availableStudents = await _studentRepository.FindEligibleStudentsForInstance(
            jobInstance.ScheduledDate,
            jobInstance.StartTime,
            jobInstance.EndTime,
            order.Senior.Contact.CityId,
            serviceIds,
            notifiedStudentIds
        );

        // Prioritize students
        return await PrioritizeStudents(
            availableStudents,
            order,
             jobInstance,
             preferedStudentId
             );
    }

    private async Task<List<Student>> PrioritizeStudents(
        List<Student> students,
         Order order,
          JobInstance jobInstance,
          int? preferedStudentId)
    {
        var senior = await _seniorRepository.GetByIdAsync(order.SeniorId);

        return students
       .OrderByDescending(s => preferedStudentId.HasValue && s.UserId == preferedStudentId.Value)
       .ThenByDescending(s => s.AverageRating)
       .ThenBy(s => senior != null ? CalculateDistance(s, senior) : double.MaxValue)
       .ToList();
    }

    private double CalculateDistance(Student student, Senior senior)
    {
        if (student?.Contact == null || senior?.Contact == null)
            return 0;

        var lat1 = (double)student.Contact.Latitude;
        var lon1 = (double)student.Contact.Longitude;
        var lat2 = (double)senior.Contact.Latitude;
        var lon2 = (double)senior.Contact.Longitude;

        const double R = 6371; // Earth radius in kilometers
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double DegreesToRadians(double deg) => deg * (Math.PI / 180);

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
                OrderScheduleId = jobInstance.Assignment.OrderScheduleId,
                OrderId = jobInstance.OrderId,
                SeniorId = jobInstance.SeniorId,
                StudentId = student.UserId,
                Status = JobRequestStatus.Pending,
                ExpiresAt = expiresAt,
                PriorityLevel = 2, // Higher priority for reassignments
                IsEmergencySub = true,
                IsReassignment = true,
                ReassignmentRecordId = reassignmentRecord.Id,
                ReassignmentType = reassignmentRecord.ReassignmentType,
                ReassignJobInstanceId = jobInstance.Id
            };

            // _jobInstanceRepository.Detach(jobInstance);
            // _reassignmentRecordRepository.Detach(reassignmentRecord);
            await _jobRequestRepository.AddAsync(jobRequest);

            // Send notification
            var jobRequestNotification = new HNotification
            {
                RecieverUserId = student.UserId,
                Title = "Urgent: Substitution Needed",
                Body = $"Urgent substitution needed for {jobInstance.ScheduledDate:MMM dd} at {jobInstance.StartTime:hh:mm tt}. " +
                      $"Expires: {expiresAt:MMM dd, yyyy hh:mm tt}",
                Type = NotificationType.JobRequest,
                Payload = JsonSerializer.Serialize(new
                {
                    InstanceId = jobInstance.Id,
                    ScheduledDate = jobInstance.ScheduledDate,
                    StartTime = jobInstance.StartTime,
                    EndTime = jobInstance.EndTime,
                    ExpiresAt = expiresAt,
                    IsReassignment = true,
                    ReassignmentType = reassignmentRecord.ReassignmentType.ToString(),
                    ReassignmentRecordId = reassignmentRecord.Id
                })
            };

            bool notificationSent = await _notificationService.SendPushNotificationAsync(
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
        reassignmentRecord.Status = ReassignmentStatus.Failed;
        reassignmentRecord.Reason += "; Max matching attempts reached";
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
        await NotifyAdminAboutReassignmentFailure(reassignmentRecord, "Max matching attempts reached");
    }

    private async Task HandleNoQualifiedStudents(JobInstance jobInstance, ReassignmentRecord reassignmentRecord)
    {
        reassignmentRecord.Status = ReassignmentStatus.Failed;
        reassignmentRecord.Reason += "; No qualified students found";
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
        await NotifyAdminAboutReassignmentFailure(reassignmentRecord, "No qualified students found");


    }

    private async Task HandleAllStudentsNotified(JobInstance jobInstance, ReassignmentRecord reassignmentRecord, List<int> notifiedStudentIds)
    {
        reassignmentRecord.Status = ReassignmentStatus.Failed;
        reassignmentRecord.Reason += $"; All {notifiedStudentIds.Count} qualified students notified, none accepted";
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
        await NotifyAdminAboutReassignmentFailure(reassignmentRecord,
            $"All {notifiedStudentIds.Count} qualified students notified, none accepted");

        // Also update the job instance status if needed
        jobInstance.NeedsSubstitute = false;
        await _jobInstanceRepository.UpdateAsync(jobInstance);
    }

    private async Task HandleMatchingFailed(ReassignmentRecord reassignmentRecord, string reason)
    {
        reassignmentRecord.Status = ReassignmentStatus.Failed;
        reassignmentRecord.Reason += $"; {reason}";
        await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

        // Notify admin
        await NotifyAdminAboutReassignmentFailure(reassignmentRecord, reason);
    }

    private async Task NotifyAdminAboutReassignmentFailure(ReassignmentRecord reassignmentRecord, string reason)
    {
        var adminId = await GetAdminId();
        var notification = new HNotification
        {
            RecieverUserId = adminId,
            Title = "Reassignment Failed",
            Body = $"Reassignment #{reassignmentRecord.Id} failed. Reason: {reason}",
            Type = NotificationType.ReassignmentFailed,
            Payload = JsonSerializer.Serialize(new
            {
                ReassignmentId = reassignmentRecord.Id,
                Reason = reason,
                AttemptCount = reassignmentRecord.AttemptCount,
                ReassignmentRecordId = reassignmentRecord.Id
            })
        };

        await _notificationService.StoreAndNotifyAsync(notification);
    }

    private Task<int> GetAdminId() => Task.FromResult(1);
}