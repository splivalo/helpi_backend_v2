using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

public class RecurringJobService : IRecurringJobService
{
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IScheduleAssignmentRepository _scheduleAssignmentRepository;
    private readonly IRecurrenceDateGenerator _dateGenerator;
    private readonly ILogger<RecurringJobService> _logger;

    public RecurringJobService(
        IJobInstanceRepository jobInstanceRepository,
        IScheduleAssignmentRepository scheduleAssignmentRepository,
        IRecurrenceDateGenerator dateGenerator,
        ILogger<RecurringJobService> logger
    )
    {
        _jobInstanceRepository = jobInstanceRepository;
        _scheduleAssignmentRepository = scheduleAssignmentRepository;
        _dateGenerator = dateGenerator;
        _logger = logger;
    }

    public async Task GenerateFutureJobInstances()
    {
        const int horizonMonths = 3;
        const int generationThresholdDays = 14;

        _logger.LogInformation("🔄 Starting future job instance generation...");

        var activeAssignments = await _scheduleAssignmentRepository.GetActiveAssignmentsAsync();
        _logger.LogInformation("📦 Retrieved {Count} active assignments", activeAssignments.Count);

        var jobInstances = new List<JobInstance>();

        foreach (var assignment in activeAssignments)
        {

            try
            {
                var instances = GenerateInstancesForAssignment(assignment, horizonMonths, generationThresholdDays);
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
        int horizonMonths = 3,
        int generationThresholdDays = 14)
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
                jobInstances.Add(new JobInstance
                {
                    SeniorId = order.SeniorId,
                    ScheduleAssignmentId = assignment.Id,
                    ScheduledDate = date,
                    StartTime = assignment.OrderSchedule.StartTime,
                    EndTime = assignment.OrderSchedule.EndTime,
                    Status = JobInstanceStatus.Upcoming
                });

                _logger.LogDebug("✅ Added job instance for {Date} (Assignment {AssignmentId})", date, assignment.Id);
            }

            return jobInstances;
        }
        catch (Exception)
        {
            _logger.LogError("❌ Failed to generate job instance for assignment {AssignmentId}threshold", assignment.Id);
            throw;
        }
    }


}
