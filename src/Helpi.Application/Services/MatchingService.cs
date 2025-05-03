



using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

public class MatchingService : IMatchingService
{

    private readonly IJobRequestRepository _jobRequestRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly StudentAvailabilitySlotService _studentAvailabilityService;
    private readonly INotificationService _notificationService;
    private readonly IOrderRepository _orderRepository;

    private readonly ILogger<MatchingService> _logger;
    private readonly IMatchingBackgroundJobs _matchingBackgroundJobs;

    public MatchingService(
        StudentAvailabilitySlotService studentAvailabilityService,
        INotificationService notificationService,
        IOrderRepository orderRepository,
        IStudentRepository studentRepository,
        IJobRequestRepository jobRequestRepository,
        IMatchingBackgroundJobs matchingBackgroundJobs,
        ILogger<MatchingService> logger
        )
    {

        _studentAvailabilityService = studentAvailabilityService;
        _notificationService = notificationService;
        _orderRepository = orderRepository;
        _studentRepository = studentRepository;
        _jobRequestRepository = jobRequestRepository;
        _matchingBackgroundJobs = matchingBackgroundJobs;
        _logger = logger;
    }

    public async Task InitiateMatchingProcessAsync(int orderId)
    {
        try
        {

            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order?.Status == OrderStatus.Pending)
            {
                // Start with first matching attempt
                await FindAndNotifyStudentsAsync(orderId);

                // Schedule next attempt if needed
                ScheduleNextMatchingAttempt(orderId, DateTime.UtcNow.AddMinutes(10));
            }
        }
        catch (Exception ex)
        {
            //  Log the error
            _logger.LogError(ex, "❌ Failed to initiate matching process for order {OrderId}", orderId);

            throw new MatchingException($"Failed to initiate matching process for order {orderId}", ex);

        }
    }

    private void ScheduleNextMatchingAttempt(int orderId, DateTime executionTime)
    {
        // Implementation depends on your scheduler 
        _matchingBackgroundJobs.ScheduleFindAndNotifyStudents(orderId, executionTime);
    }

    public async Task FindAndNotifyStudentsAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null) return;

        var qualifiedStudents = await FindQualifiedStudents(order);


        var firstStudent = qualifiedStudents.First();

        await TrySendJobRequest(firstStudent, order.Schedules.First());
    }

    private async Task<List<Student>> FindQualifiedStudents(Order order)
    {
        // 1. Get required service IDs
        var serviceIds = order.OrderServices.Select(s => s.ServiceId).ToList();


        // 2. Find students qualified for ALL services
        var firstSchedule = order.Schedules.First();

        var notifiedStudentIds = await _jobRequestRepository.NotifiedStudentIds(firstSchedule.Id);

        /// todo: use _studentRepository.GetAvailableStudentsForOrderSchedule to keep logic for finding student in one place
        var students = await _studentRepository.UnnotifiedStudentsOfferingServices(
                                serviceIds,
                                notifiedStudentIds
                                );


        // 3. Filter by availability
        // return await _availabilityService.FilterByAvailability(
        //     students,
        //     order.Schedules);

        return students;
    }

    private async Task<bool> TrySendJobRequest(Student student, OrderSchedule orderSchedule)
    {
        // Create JobRequest entity
        var jobRequest = new JobRequest
        {
            OrderScheduleId = orderSchedule.Id,
            StudentId = student.UserId,
            Status = JobRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _jobRequestRepository.AddAsync(jobRequest);



        // Send notification
        return await _notificationService.SendJobRequestNotification(
            student.UserId,
            orderSchedule.Id,
            jobRequest.ExpiresAt);
    }
}