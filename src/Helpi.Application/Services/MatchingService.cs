



using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

public class MatchingService : IMatchingService
{

    private readonly IJobRequestRepository _jobRequestRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly StudentAvailabilitySlotService _studentAvailabilityService;
    private readonly INotificationService _notificationService;
    private readonly IOrderRepository _orderRepository;


    private readonly IMatchingBackgroundJobs _matchingBackgroundJobs;

    public MatchingService(
        StudentAvailabilitySlotService studentAvailabilityService,
        INotificationService notificationService,
        IOrderRepository orderRepository,
        IStudentRepository studentRepository,
        IJobRequestRepository jobRequestRepository,
        IMatchingBackgroundJobs matchingBackgroundJobs
        )
    {

        _studentAvailabilityService = studentAvailabilityService;
        _notificationService = notificationService;
        _orderRepository = orderRepository;
        _studentRepository = studentRepository;
        _jobRequestRepository = jobRequestRepository;
        _matchingBackgroundJobs = matchingBackgroundJobs;
    }

    public async Task InitiateMatchingProcessAsync(int orderId)
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