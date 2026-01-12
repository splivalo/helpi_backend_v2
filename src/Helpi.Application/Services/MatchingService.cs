
using System.Text.Json;
using System.Threading.Tasks;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class MatchingService : IMatchingService
{
    private readonly IJobRequestRepository _jobRequestRepository;

    private readonly IScheduleAssignmentRepository _scheduleAssignmentRepository;
    private readonly IOrderScheduleRepository _orderScheduleRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISeniorRepository _seniorRepository;
    private readonly StudentAvailabilitySlotService _studentAvailabilityService;
    private readonly INotificationService _notificationService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<MatchingService> _logger;
    private readonly IMatchingBackgroundJobs _matchingBackgroundJobs;

    private readonly IReassignmentRecordRepository _reassignmentRecordRepository;

    // Configuration parameters  could be moved to app settings
    private readonly int _maxConcurrentNotifications = 1;  // Number of students to notify at once
    private readonly int _jobRequestExpirationMinutes = 10;
    private readonly int _retryIntervalMinutes = 10; /// todo: 10
    private readonly int _maxMatchingAttempts = 100000;

    private readonly IServiceScopeFactory _scopeFactory;

    public MatchingService(
        StudentAvailabilitySlotService studentAvailabilityService,
        IScheduleAssignmentRepository scheduleAssignmentRepository,
        IOrderScheduleRepository orderScheduleRepository,
        INotificationService notificationService,
INotificationFactory notificationFactory,
        IOrderRepository orderRepository,
        IStudentRepository studentRepository,
        ISeniorRepository seniorRepository,
        IJobRequestRepository jobRequestRepository,
        IMatchingBackgroundJobs matchingBackgroundJobs,
        IReassignmentRecordRepository reassignmentRecordRepository,
        ILogger<MatchingService> logger,
        IServiceScopeFactory scopeFactory
        )
    {
        _studentAvailabilityService = studentAvailabilityService;
        _scheduleAssignmentRepository = scheduleAssignmentRepository;
        _orderScheduleRepository = orderScheduleRepository;
        _notificationService = notificationService;
        _notificationFactory = notificationFactory;
        _orderRepository = orderRepository;
        _studentRepository = studentRepository;
        _seniorRepository = seniorRepository;
        _jobRequestRepository = jobRequestRepository;
        _matchingBackgroundJobs = matchingBackgroundJobs;
        _reassignmentRecordRepository = reassignmentRecordRepository;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }



    public async Task StartMatching(int orderId)
    {

        await ScheduleNextMatchingAttempt(orderId, DateTime.UtcNow.AddSeconds(5));

    }



    public async Task InitiateMatchingProcessAsync(int orderId)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                _logger.LogWarning("⚠️ Cannot initiate matching for non-existent order {OrderId}", orderId);
                return;
            }

            var unassignedSchedules = await UnassignedSchedulesAsync(order.Schedules);

            if (unassignedSchedules.Any())
            {

                // Process all schedules in the order
                await ProcessAllSchedulesAsync(order);


                await ScheduleNextMatchingAttempt(orderId, DateTime.UtcNow.AddMinutes(_retryIntervalMinutes));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initiate matching process for order {OrderId}", orderId);
            throw new MatchingException($"Failed to initiate matching process for order {orderId}", ex);
        }
    }


    private async Task ScheduleNextMatchingAttempt(int orderId, DateTime executionTime)
    {

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        var jobId = _matchingBackgroundJobs.ScheduleFindAndNotifyStudents(
            orderId,
            order.HangFireMatchingJobId,
            executionTime);

        if (jobId != null)
        {
            order.HangFireMatchingJobId = jobId;
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("⏰ Scheduled next matching attempt for order {OrderId} at {Time}",
                orderId, executionTime);
        }
        // using var scope = _scopeFactory.CreateScope();
        // var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        // var matchingJobs = scope.ServiceProvider.GetRequiredService<IMatchingBackgroundJobs>();

        // var order = await orderRepository.LoadOrderWithIncludes(orderId, new OrderIncludeOptions { });
        // if (order == null) return;


        // var jobId = matchingJobs.ScheduleFindAndNotifyStudents(
        //                         orderId,
        //                         order.HangFireMatchingJobId,
        //                             executionTime);

        // if (jobId != null)
        // {
        //     order.HangFireMatchingJobId = jobId;
        //     await orderRepository.UpdateAsync(order);

        //     _logger.LogInformation("⏰ Scheduled next matching attempt for order {OrderId} at {Time}",
        //     orderId, executionTime);
        // }
    }

    private async Task<ICollection<OrderSchedule>> UnassignedSchedulesAsync(ICollection<OrderSchedule> schedules)
    {

        ICollection<OrderSchedule> unassigned = [];

        foreach (var schedule in schedules)
        {
            if (schedule.IsCancelled) continue;

            var pendingReassignment = await IsSchedulesPendingReassignAsync(schedule);

            if (pendingReassignment)
            {
                unassigned.Add(schedule);
                continue;
            }

            if (schedule.AllowAutoScheduling == false)
            {
                _logger.LogInformation("⚠️ [allowAutoScheduling]  = FALSE -> schedule {schedule}", schedule.Id);
                continue;
            }

            // Check if schedule is already assigned
            bool isAssigned = await _scheduleAssignmentRepository.IsScheduleAssigned(schedule.Id);

            if (isAssigned)
            {
                _logger.LogInformation("📅 Schedule {ScheduleId} is already assigned, skipping", schedule.Id);
                continue;
            }
            else
            {
                _logger.LogInformation("📅⚠️ Schedule {ScheduleId} is not assigned", schedule.Id);
                unassigned.Add(schedule);
                continue;
            }

        }

        return unassigned;
    }

    private async Task<bool> IsSchedulesPendingReassignAsync(OrderSchedule schedule)
    {

        var reassignmentRecord = await _reassignmentRecordRepository.GetScheduleActiveReassignmentAsync(
               schedule.Id);

        if (reassignmentRecord != null)
        {
            _logger.LogInformation("📅 Schedule {ScheduleId} is pending assignement, {record}", schedule.Id, reassignmentRecord.Id);

            return true;
        }

        return false;

    }



    private async Task ProcessAllSchedulesAsync(Order order)
    {
        foreach (var schedule in order.Schedules)
        {
            var reassignmentRecord = await _reassignmentRecordRepository.GetScheduleActiveReassignmentAsync(
                       schedule.Id);

            int currentAttempt = 0;

            if (reassignmentRecord != null)
            {
                _logger.LogWarning("⚠️ This is reassignment matching for  record {RecordId}", reassignmentRecord.Id);
                currentAttempt = reassignmentRecord.AttemptCount;
                if (reassignmentRecord.AllowAutoScheduling == false) return;
            }
            else
            {
                if (schedule.AllowAutoScheduling == false) return;
                currentAttempt = schedule.AutoScheduleAttemptCount;
            }


            if (currentAttempt >= _maxMatchingAttempts)
            {
                _logger.LogWarning("⚠️ Max matching attempts reached for {ScheduleId} in order {OrderId}", schedule.Id, order.Id);
                await HandleMaxAttemptsReached(schedule);
                return;
            }

            await FindAndNotifyStudentsForScheduleAsync(order, schedule, reassignmentRecord);

            // Increment the attempt counter and schedule next attempt if needed
            await IncrementMatchingAttemptCount(schedule, reassignmentRecord);
        }
    }



    public async Task FindAndNotifyStudentsAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        // Process all schedules
        await ProcessAllSchedulesAsync(order);
    }

    private async Task FindAndNotifyStudentsForScheduleAsync(
        Order order,
        OrderSchedule schedule,
        ReassignmentRecord? reassignment = null)
    {
        try
        {
            // Check if this is a reassignment scenario
            if (reassignment != null)
            {
                _logger.LogInformation("🔁 Processing reassignment for schedule {ScheduleId}, Record: {RecordId}",
                    schedule.Id, reassignment.Id);
            }

            // Get students who have already been notified for this schedule     
            List<int> notifiedStudentIds = await GetNotifiedStudentsIds(schedule.Id, reassignment);

            var qualifiedStudents = await FindQualifiedStudentsForSchedule(order, schedule, notifiedStudentIds, reassignment);

            if (!qualifiedStudents.Any())
            {
                if (notifiedStudentIds.Any())
                {
                    _logger.LogInformation("📝  All Eligable  Students Notified  for schedule {ScheduleId}", schedule.Id);
                    await HandleAllEligableStudentsNotified(order, schedule, reassignment);
                    return;
                }
                else
                {
                    await HandleNoEligableStudents(order, schedule, reassignment);
                    return;
                }
            }



            // Filter out already notified students
            var unnotifiedStudents = qualifiedStudents;


            // Take the top N students based on our prioritization
            var studentsToNotify = unnotifiedStudents.Take(_maxConcurrentNotifications).ToList();



            // Process each student one at a time to avoid context disposal issues
            foreach (var student in studentsToNotify)
            {
                await TrySendJobRequest(student, order, schedule, reassignment);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding and notifying students for schedule {ScheduleId}", schedule.Id);
        }
    }



    private async Task<List<int>> GetNotifiedStudentsIds(
        int scheduleId,
        ReassignmentRecord? reassignment = null)
    {


        List<int> notifiedStudentIds = [];
        if (reassignment != null)
        {
            notifiedStudentIds = await _jobRequestRepository.NotifiedStudentIdsForReassignment(reassignment.Id);

            // Also exclude the original student if this is a reassignment
            // if (reassignment.OriginalStudentId.HasValue)
            // {
            // notifiedStudentIds.Add(reassignment.OriginalStudentId.Value);
            // }
        }
        else
        {
            notifiedStudentIds = await _jobRequestRepository.NotifiedStudentIds(scheduleId);
        }

        return notifiedStudentIds;
    }
    private async Task<List<Student>> FindQualifiedStudentsForSchedule(
        Order order,
        OrderSchedule schedule,
        List<int> notifiedStudentIds,
        ReassignmentRecord? reassignment = null
        )
    {
        // 1. Get required service IDs
        var serviceIds = order.OrderServices.Select(s => s.ServiceId).ToList();

        // 2. Find students qualified for ALL services who haven't been notified yet
        var availableStudents = await _studentRepository.FindEligibleStudentsForSchedule(
            schedule.Id,
            notifiedStudentIds,
            preferedStudentId: reassignment?.PreferredStudentId
        );



        // 3. Prioritize students
        return availableStudents;
    }

    private async Task<bool> TrySendJobRequest(
        Student student,
        Order order,
        OrderSchedule orderSchedule,
        ReassignmentRecord? reassignment)
    {
        try
        {

            var expiresAt = DateTime.UtcNow.AddMinutes(_jobRequestExpirationMinutes);

            // Create JobRequest entity
            var jobRequest = new JobRequest
            {
                OrderScheduleId = orderSchedule.Id,
                OrderId = order.Id,
                SeniorId = order.SeniorId,
                StudentId = student.UserId,
                Status = JobRequestStatus.Pending,
                ExpiresAt = expiresAt,
                PriorityLevel = reassignment != null ? (byte)2 : (byte)1,
                IsReassignment = reassignment != null,
                ReassignmentRecordId = reassignment?.Id,
                ReassignmentType = reassignment?.ReassignmentType,
                ReassignAssignmentId = reassignment?.ReassignAssignmentId,
                ReassignJobInstanceId = reassignment?.ReassignJobInstanceId
            };

            await _jobRequestRepository.AddAsync(jobRequest);

            // Send notification
            var studentCulture = student.Contact.LanguageCode ?? "en";
            var jobRequestNotification = _notificationFactory.JobRequestNotification(student.UserId, orderSchedule, reassignment, culture: studentCulture);


            bool notificationSent = await _notificationService.SendNotificationAsync(
                student.UserId,
                jobRequestNotification);

            if (notificationSent)
            {
                _logger.LogInformation("✅ Job request sent to student {StudentId} for schedule {ScheduleId}",
                    student.UserId, orderSchedule.Id);
            }


            return notificationSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending job request to student {StudentId} for schedule {ScheduleId}",
                student.UserId, orderSchedule.Id);
            return false;
        }
    }


    private async Task IncrementMatchingAttemptCount(OrderSchedule schedule, ReassignmentRecord? reassignmentRecord)
    {
        if (reassignmentRecord != null)
        {
            reassignmentRecord.AttemptCount = reassignmentRecord.AttemptCount + 1;
            await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);
        }
        else
        {
            schedule.AutoScheduleAttemptCount = schedule.AutoScheduleAttemptCount + 1;
            await _orderScheduleRepository.UpdateAsync(schedule);
        }

    }
    private async Task MarkAutoSchedulingStatusAsync(
        OrderSchedule schedule,
        AutoScheduleDisableReason? scheduleReason,
        ReassignmentRecord? reassignmentRecord,
        bool recordAllowAutoScheduling,
         ReassignmentStatus? recordReason
        )
    {

        try
        {
            if (reassignmentRecord != null)
            {
                reassignmentRecord.Status = recordReason!.Value;
                reassignmentRecord.AllowAutoScheduling = recordAllowAutoScheduling;
                await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);
            }
            else
            {
                schedule.AllowAutoScheduling = false;
                schedule.AutoScheduleDisableReason = scheduleReason ?? AutoScheduleDisableReason.noEligibleStudents;
                await _orderScheduleRepository.UpdateAsync(schedule);
            }
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "❌ Failed [MarkAutoMatchingFail({schedule} , {reassing}) ", schedule.Id, reassignmentRecord?.Id);
        }

    }

    private async Task HandleMaxAttemptsReached(OrderSchedule schedule)
    {
        // Notify admin or customer that no matches were found after all attempts
        // Could automatically offer incentives or adjust requirements
        await Task.CompletedTask; // Placeholder
    }

    private async Task HandleNoEligableStudents(
        Order order,
        OrderSchedule schedule,
        ReassignmentRecord? reassignment = null)
    {
        _logger.LogWarning("⚠️ No qualified students found for schedule {ScheduleId}", schedule.Id);


        var adminId = await GetAdminId();

        var notification = _notificationFactory.NoEligibleStudentsNotification(
                                adminId,
                                order,
                                schedule,
                                reassignment
                                );





        await _notificationService.StoreAndNotifyAsync(notification);

        ///
        // Update schedule or reassignment record

        await MarkAutoSchedulingStatusAsync(
                schedule,
                AutoScheduleDisableReason.noEligibleStudents,
                reassignment,
                recordAllowAutoScheduling: false,
                ReassignmentStatus.NoEligibleStudents
               );


    }

    /// None of the notified students accepted
    private async Task HandleAllEligableStudentsNotified(
        Order order,
        OrderSchedule schedule,
        ReassignmentRecord? reassignment = null)
    {


        var adminId = await GetAdminId();

        var notification = _notificationFactory.AllEligibleStudentsNotified(
                                    adminId,
                                    order,
                                    schedule,
                                    reassignment
                                    );

        await _notificationService.StoreAndNotifyAsync(notification);

        // Update schedule or reassignment record
        await MarkAutoSchedulingStatusAsync(
                schedule,
                AutoScheduleDisableReason.noEligibleStudents,
                reassignment,
                recordAllowAutoScheduling: false,
                ReassignmentStatus.InProgress
               );
    }

    Task<int> GetAdminId()
    {
        return Task.FromResult(1);
    }
}