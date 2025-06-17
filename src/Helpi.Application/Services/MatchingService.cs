
using System.Text.Json;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
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
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<MatchingService> _logger;
    private readonly IMatchingBackgroundJobs _matchingBackgroundJobs;

    // Configuration parameters  could be moved to app settings
    private readonly int _maxConcurrentNotifications = 1;  // Number of students to notify at once
    private readonly int _jobRequestExpirationMinutes = 10;
    private readonly int _retryIntervalMinutes = 10; /// todo: 10
    private readonly int _maxMatchingAttempts = 100000;

    public MatchingService(
        StudentAvailabilitySlotService studentAvailabilityService,
        IScheduleAssignmentRepository scheduleAssignmentRepository,
        IOrderScheduleRepository orderScheduleRepository,
        INotificationService notificationService,
        IOrderRepository orderRepository,
        IStudentRepository studentRepository,
        ISeniorRepository seniorRepository,
        IJobRequestRepository jobRequestRepository,
        IMatchingBackgroundJobs matchingBackgroundJobs,
        ILogger<MatchingService> logger)
    {
        _studentAvailabilityService = studentAvailabilityService;
        _scheduleAssignmentRepository = scheduleAssignmentRepository;
        _orderScheduleRepository = orderScheduleRepository;
        _notificationService = notificationService;
        _orderRepository = orderRepository;
        _studentRepository = studentRepository;
        _seniorRepository = seniorRepository;
        _jobRequestRepository = jobRequestRepository;
        _matchingBackgroundJobs = matchingBackgroundJobs;
        _logger = logger;
    }

    public void StartMatching(int orderId)
    {
        ScheduleNextMatchingAttempt(orderId, DateTime.UtcNow.AddSeconds(5));
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


                ScheduleNextMatchingAttempt(orderId, DateTime.UtcNow.AddMinutes(_retryIntervalMinutes));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initiate matching process for order {OrderId}", orderId);
            throw new MatchingException($"Failed to initiate matching process for order {orderId}", ex);
        }
    }

    private async Task<ICollection<OrderSchedule>> UnassignedSchedulesAsync(ICollection<OrderSchedule> schedules)
    {

        ICollection<OrderSchedule> unassigned = [];

        foreach (var schedule in schedules)
        {
            if (schedule.IsCancelled) continue;

            if (schedule.allowAutoScheduling == false)
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

    private async Task ProcessAllSchedulesAsync(Order order)
    {
        foreach (var schedule in order.Schedules)
        {
            if (schedule.allowAutoScheduling == false) return;

            int currentAttempt = schedule.AutoScheduleAttemptCount;

            if (currentAttempt >= _maxMatchingAttempts)
            {
                _logger.LogWarning("⚠️ Max matching attempts reached for {ScheduleId} in order {OrderId}", schedule.Id, order.Id);
                await HandleMaxAttemptsReached(schedule);
                return;
            }

            await FindAndNotifyStudentsForScheduleAsync(order, schedule);

            // Increment the attempt counter and schedule next attempt if needed
            await IncrementMatchingAttemptCount(schedule);
        }
    }



    public async Task FindAndNotifyStudentsAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        // Process all schedules
        await ProcessAllSchedulesAsync(order);
    }

    private async Task FindAndNotifyStudentsForScheduleAsync(Order order, OrderSchedule schedule)
    {
        try
        {
            var qualifiedStudents = await FindQualifiedStudentsForSchedule(order, schedule);

            if (!qualifiedStudents.Any())
            {

                HandleNoEligableStudents(order, schedule);
                return;
            }

            // Get students who have already been notified for this schedule
            var notifiedStudentIds = await _jobRequestRepository.NotifiedStudentIds(schedule.Id);

            // Filter out already notified students
            var unnotifiedStudents = qualifiedStudents
                .Where(s => !notifiedStudentIds.Contains(s.UserId))
                .ToList();

            if (!unnotifiedStudents.Any())
            {
                _logger.LogInformation("📝 Out of qualified students  for schedule {ScheduleId}", schedule.Id);
                HandleEligableAllStudentsNotified(order, schedule, notifiedStudentIds);
                return;
            }

            // Take the top N students based on our prioritization
            var studentsToNotify = unnotifiedStudents.Take(_maxConcurrentNotifications).ToList();



            // Process each student one at a time to avoid context disposal issues
            foreach (var student in studentsToNotify)
            {
                await TrySendJobRequest(student, order, schedule);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding and notifying students for schedule {ScheduleId}", schedule.Id);
        }
    }

    private async Task<List<Student>> FindQualifiedStudentsForSchedule(Order order, OrderSchedule schedule)
    {
        // 1. Get required service IDs
        var serviceIds = order.OrderServices.Select(s => s.ServiceId).ToList();

        // 2. Find students qualified for ALL services who haven't been notified yet
        var notifiedStudentIds = await _jobRequestRepository.NotifiedStudentIds(schedule.Id);

        var availableStudents = await _studentRepository.FindEligibleStudentsForSchedule(
            schedule.Id,
            notifiedStudentIds
        );



        // 3. Prioritize students
        return await PrioritizeStudents(availableStudents, order, schedule);
    }

    private async Task<List<Student>> PrioritizeStudents(List<Student> students, Order order, OrderSchedule schedule)
    {

        var senior = await _seniorRepository.GetByIdAsync(order.SeniorId);

        // 1. Find students who've worked with this senior before
        // var previouslyWorkedWith = await _studentRepository.StudentsWhoWorkedWithSenior(order.SeniorId);
        // var prioritized = students
        //     .OrderByDescending(s => previouslyWorkedWith.Contains(s.Id)) 
        //     .ThenByDescending(s => s.AverageRating)                     
        //     .ThenBy(s => senior != null ? CalculateDistance(s, senior) : double.MaxValue) 
        //     .ToList();

        var prioritized = students
       .OrderByDescending(s => s.AverageRating)
       .ThenBy(s => senior != null ? CalculateDistance(s, senior) : double.MaxValue)
       .ToList();

        return prioritized;

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


    private async Task<bool> TrySendJobRequest(Student student, Order order, OrderSchedule orderSchedule)
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
                PriorityLevel = 1, // Could vary based on matching attempt number
                IsEmergencySub = false // Set true for urgent cases
            };

            await _jobRequestRepository.AddAsync(jobRequest);

            // Send notification

            var jobRequestNotification = new HNotification
            {
                RecieverUserId = student.UserId,
                Title = "Job request",
                Body = $"Expires: {expiresAt:MMM dd, yyyy hh:mm tt}",
                Type = NotificationType.JobRequest,
                Payload = JsonSerializer.Serialize(new
                {
                    OrderSchedule = orderSchedule.Id,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                })
            };


            bool notificationSent = await _notificationService.SendPushNotificationAsync(
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

    private void ScheduleNextMatchingAttempt(int orderId, DateTime executionTime)
    {
        _matchingBackgroundJobs.ScheduleFindAndNotifyStudents(orderId, executionTime);
        _logger.LogInformation("⏰ Scheduled next matching attempt for order {OrderId} at {Time}",
            orderId, executionTime);
    }

    private async Task IncrementMatchingAttemptCount(OrderSchedule schedule)
    {
        schedule.AutoScheduleAttemptCount = schedule.AutoScheduleAttemptCount + 1;
        await _orderScheduleRepository.UpdateAsync(schedule);
    }

    private async Task HandleMaxAttemptsReached(OrderSchedule schedule)
    {
        // Notify admin or customer that no matches were found after all attempts
        // Could automatically offer incentives or adjust requirements
        await Task.CompletedTask; // Placeholder
    }

    private async Task HandleNoEligableStudents(Order order, OrderSchedule schedule)
    {
        _logger.LogWarning("⚠️ No qualified students found for schedule {ScheduleId}", schedule.Id);

        /// todo : stop from being scheduled again

        var adminId = await GetAdminId();

        var notification = new HNotification
        {
            RecieverUserId = adminId,
            Title = "No Eligible Students Available",
            Body = $"Schedule #{schedule.Id} could not be assigned due to a lack of eligible students.",
            Type = NotificationType.NoEligibleStudents,
            Payload = JsonSerializer.Serialize(new
            {
                Order = order.Id,
                Schedule = schedule.Id,
            })
        };



        await _notificationService.StoreAndNotifyAsync(notification);

        ///
        schedule.allowAutoScheduling = false;
        schedule.AutoScheduleDisableReason = AutoScheduleDisableReason.noEligibleStudents;
        await _orderScheduleRepository.UpdateAsync(schedule);
    }

    /// None of the notified students accepted
    private async Task HandleEligableAllStudentsNotified(Order order, OrderSchedule schedule, List<int> notifiedStudents)
    {
        /// todo : stop from being scheduled again

        var adminId = await GetAdminId();

        var notification = new HNotification
        {
            RecieverUserId = adminId,
            Title = "Schedule Assignment Unaccepted",
            Body = $"All {notifiedStudents.Count()} eligable students notified, none accepted yet",
            Type = NotificationType.NoEligableStudentAcceptedJobYet,
            Payload = JsonSerializer.Serialize(new
            {
                Order = order.Id,
                Schedule = schedule.Id,
                NotifiedStudent = notifiedStudents,
            })
        };

        await _notificationService.StoreAndNotifyAsync(notification);


        ///
        schedule.allowAutoScheduling = false;
        schedule.AutoScheduleDisableReason = AutoScheduleDisableReason.allEligibleStudentsNotified;
        await _orderScheduleRepository.UpdateAsync(schedule);
    }

    Task<int> GetAdminId()
    {
        return Task.FromResult(1);
    }
}