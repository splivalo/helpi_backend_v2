using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Utilities;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

public class HangfireRecurringJobService : IHangfireRecurringJobService
{
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IScheduleAssignmentRepository _scheduleAssignmentRepository;
    private readonly IPricingConfigurationRepository _pricingConfig;
    private readonly IRecurrenceDateGenerator _dateGenerator;
    private readonly ILogger<HangfireRecurringJobService> _logger;

    private readonly IHangfireService _hangfireService;

    public HangfireRecurringJobService(
        IJobInstanceRepository jobInstanceRepository,
        IScheduleAssignmentRepository scheduleAssignmentRepository,
        IPricingConfigurationRepository pricingConfig,
        IRecurrenceDateGenerator dateGenerator,
        ILogger<HangfireRecurringJobService> logger,
        IHangfireService hangfireService
    )
    {
        _jobInstanceRepository = jobInstanceRepository;
        _scheduleAssignmentRepository = scheduleAssignmentRepository;
        _pricingConfig = pricingConfig;
        _dateGenerator = dateGenerator;
        _logger = logger;
        _hangfireService = hangfireService;
    }

    public async Task GenerateFutureJobInstances()
    {
        const int horizonMonths = 3;
        const int generationThresholdDays = 14;

        _logger.LogInformation("🔄 Starting future job instance generation...");

        var activeAssignments = await _scheduleAssignmentRepository.GetAssignmentsNeedingJobGenerationAsync();
        _logger.LogInformation("📦 Retrieved {Count} active assignments", activeAssignments.Count);

        var pricingConfig = await _pricingConfig.GetByIdAsync(1);

        if (pricingConfig == null)
        {
            _logger.LogInformation("❌ Not pricing configuration found");
            return;
        }

        var jobInstances = new List<JobInstance>();

        foreach (var assignment in activeAssignments)
        {

            try
            {
                var instances = GenerateInstancesForAssignment(
                    assignment,
                     pricingConfig,
                 horizonMonths,
                 generationThresholdDays
                );

                jobInstances.AddRange(instances);
            }
            catch (Exception)
            {
            }

        }

        if (jobInstances.Any())
        {
            _logger.LogInformation("💾 Saving {Count} new job instances...", jobInstances.Count);
            await _jobInstanceRepository.AddRangeAsync(jobInstances);
            _logger.LogInformation("🎉 Job instance generation complete!");
        }
        else
        {
            _logger.LogInformation("🟡 No job instances to generate.");
        }
    }

    public List<JobInstance> GenerateInstancesForAssignment(
        ScheduleAssignment assignment,
         PricingConfiguration pricingConfiguration,
        int horizonMonths = 3,
        int generationThresholdDays = 14

        )
    {
        try
        {
            var jobInstances = new List<JobInstance>();
            var order = assignment.OrderSchedule.Order;

            var lastInstanceDate = assignment.JobInstances.Any()
            ? assignment.JobInstances.Max(ji => ji.ScheduledDate)
            : order.StartDate.AddDays(-1);

            var horizonDate = DateOnly
                .FromDateTime(DateTime.UtcNow)
                .AddMonths(horizonMonths)
                .AddDays(-generationThresholdDays);

            _logger.LogDebug(
                "📅 Last instance for assignment {AssignmentId} is {LastDate}, horizon threshold is {HorizonDate}",
                assignment.Id,
                lastInstanceDate,
                horizonDate
                );

            if (lastInstanceDate > horizonDate)
            {
                _logger.LogInformation("⏭️ Skipping assignment {AssignmentId} – already generated up to threshold", assignment.Id);
                return jobInstances;
            }

            var startDate = lastInstanceDate.AddDays(1);
            var dayOfWeek = (DayOfWeek)assignment.OrderSchedule.DayOfWeek;




            _logger.LogInformation("🛠 Generating dates for assignment {AssignmentId} starting from {StartDate}", assignment.Id, startDate);


            var newDates = _dateGenerator.GetDates(
                startDate,
                order.EndDate,
                order.RecurrencePattern,
                dayOfWeek,
                horizonMonths
            );

            var dateList = string.Join(", ", newDates.Select(d => d.ToString("yyyy-MM-dd")));

            _logger.LogInformation(
                "📅 [JOB INSTANCE GEN]\n➡️ AssignmentId: {AssignmentId}\n🔄 Recurring: {recuring}\n🧮 Days Count: {Count}\n🗓️ Dates: {DateList}",
                assignment.Id,
                order.RecurrencePattern.ToString(),
                newDates.Count(),
                dateList);

            foreach (var date in newDates)
            {
                var isOvertimeDay = date.DayOfWeek == DayOfWeek.Sunday
                    || CroatianHolidays.IsPublicHoliday(date);
                var hourlyRate = isOvertimeDay
                    ? pricingConfiguration.SundayHourlyRate
                    : pricingConfiguration.JobHourlyRate;

                jobInstances.Add(new JobInstance
                {
                    SeniorId = order.SeniorId,
                    CustomerId = order.Senior.CustomerId,
                    OrderId = order.Id,
                    Notes = order.Notes,
                    OrderScheduleId = assignment.OrderScheduleId,
                    HourlyRate = hourlyRate,
                    CompanyPercentage = pricingConfiguration.CompanyPercentage,
                    ServiceProviderPercentage = pricingConfiguration.ServiceProviderPercentage,
                    ScheduleAssignmentId = assignment.Id,
                    ScheduledDate = date,
                    StartTime = assignment.OrderSchedule.StartTime,
                    EndTime = assignment.OrderSchedule.EndTime,
                    Status = JobInstanceStatus.Upcoming
                });

                _logger.LogDebug("✅ Added job instance for {Date} (Assignment {AssignmentId}, Overtime={IsOvertime}, Rate={Rate})",
                    date, assignment.Id, isOvertimeDay, hourlyRate);
            }

            return jobInstances;
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Failed to generate job instance for assignment {AssignmentId}", assignment.Id);
            _logger.LogError(ex.ToString());
            throw;
        }
    }



    /// ==============================
    /// 


    /// <summary>
    /// Schedules Hangfire background jobs to automatically update the status of today's job instances 
    /// to "InProgress" at their start time and to "Completed" at their end time.
    /// </summary>
    /// <remarks>
    /// - Fetches all job instances scheduled for today.
    /// - Uses Hangfire to enqueue status update tasks at the appropriate times.
    /// - Stores the scheduled job IDs on each instance for potential tracking or cancellation.
    /// </remarks>
    public async Task ScheduleDailyJobInstanceStatusUpdates()
    {
        _logger.LogInformation("📅 Starting daily status update scheduling at {Time}.", DateTime.UtcNow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var jobInstancesForToday = await _jobInstanceRepository.GetByDateAsync(today);

        if (jobInstancesForToday == null || !jobInstancesForToday.Any())
        {
            _logger.LogInformation("🚫 No job instances scheduled for today ({Date}).", today);
            return;
        }

        _logger.LogInformation("✅ Found {Count} job instance(s) scheduled for today ({Date}).", jobInstancesForToday.Count, today);

        foreach (var instance in jobInstancesForToday)
        {


            if (instance.Status != JobInstanceStatus.Upcoming)
            {
                _logger.LogInformation("🚫[Skip] Job #{id} -> status {status} != Upcoming", instance.Id, instance.Status);
                continue;
            }



            var startTime = DateTimeUtils.ToUtc(instance.ScheduledDate, instance.StartTime);
            var endTime = DateTimeUtils.ToUtc(instance.ScheduledDate, instance.EndTime);


            _logger.LogDebug("⏳ Scheduling JobInstance #{Id}: ▶️ InProgress at {StartTime}, ✅ Completed at {EndTime}.",
                instance.Id, startTime, endTime);

            instance.HangFireStartStatusJobId = _hangfireService.Schedule<IJobInstanceService>(
                s => s.UpdateToInProgressAsync(instance.Id),
                startTime
            );

            instance.HangFireEndStatusJobId = _hangfireService.Schedule<IJobInstanceService>(
                s => s.UpdateToCompletedAsync(instance.Id),
                endTime
            );


            var remindAt = startTime.AddMinutes(-30);
            instance.HangFireRemindStudentJobId = _hangfireService.Schedule<IJobInstanceService>(
                s => s.RemindStudentAsync(instance.Id),
                remindAt
            );



            _logger.LogDebug("📌 JobInstance #{Id} scheduled — StartJobId: {StartJobId} 🟢, EndJobId: {EndJobId} 🔴.",
                instance.Id,
                instance.HangFireStartStatusJobId,
                instance.HangFireEndStatusJobId);
        }

        await _jobInstanceRepository.SaveChangesAsync();

        _logger.LogInformation("🎯 Finished scheduling status updates for today’s job instances.");
    }

    public async Task ScheduleDailyJobInstancePayments()
    {
        _logger.LogInformation("📅 Starting daily job payments scheduling at {Time}.", DateTime.UtcNow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var jobInstancesForToday = await _jobInstanceRepository.GetByDateAsync(today);

        if (jobInstancesForToday == null || !jobInstancesForToday.Any())
        {
            _logger.LogInformation("🚫 No job instances scheduled for today ({Date}).", today);
            return;
        }

        _logger.LogInformation("✅ Found {Count} job instance(s) scheduled for today ({Date}).", jobInstancesForToday.Count, today);

        foreach (var instance in jobInstancesForToday)
        {
            if (instance.Status != JobInstanceStatus.Upcoming)
            {
                _logger.LogInformation("🚫[Skip] Job #{id} -> status {status} != Upcoming", instance.Id, instance.Status);
                continue;
            }

            var startTime = DateTimeUtils.ToUtc(instance.ScheduledDate, instance.StartTime);
            var endTime = DateTimeUtils.ToUtc(instance.ScheduledDate, instance.EndTime);


            var chargePaymentAt = startTime.AddMinutes(-30); // this has to happen before RemindStudent
            _logger.LogDebug("⏳ Scheduling Payment -> JobInstance #{Id}.",
                instance.Id);

            instance.HangFirePaymentJobId = _hangfireService.Schedule<IPaymentService>(
                s => s.ProcessPaymentAsync(instance.Id),
                chargePaymentAt
            );

            _logger.LogDebug("📌 JobInstance #{Id} scheduled payment — hangfirePaymentJobId: {hangfirePaymentJobId}",
                instance.Id,
                instance.HangFirePaymentJobId);
        }

        await _jobInstanceRepository.SaveChangesAsync();

        _logger.LogInformation("🎯 Finished scheduling Payments updates for today’s job instances.");
    }

}
