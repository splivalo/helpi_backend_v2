using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

public class RecurringJobService
{
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IScheduleAssignmentRepository _scheduleAssignmentRepository;
    private readonly IRecurrenceDateGenerator _dateGenerator;

    public RecurringJobService(IJobInstanceRepository jobInstanceRepository,
    IScheduleAssignmentRepository scheduleAssignmentRepository,
     IRecurrenceDateGenerator dateGenerator)
    {
        _jobInstanceRepository = jobInstanceRepository;
        _scheduleAssignmentRepository = scheduleAssignmentRepository;
        _dateGenerator = dateGenerator;
    }

    public async Task GenerateFutureJobInstances()
    {
        const int horizonMonths = 3;
        const int generationThresholdDays = 14; // Generate when remaining < 2 weeks

        var jobInstances = new List<JobInstance>();



        var activeAssignments = await _scheduleAssignmentRepository.GetActiveAssignmentsAsync();




        foreach (var assignment in activeAssignments)
        {
            var order = assignment.OrderSchedule.Order;
            var lastInstanceDate = assignment.JobInstances
                .Max(ji => ji.ScheduledDate);

            // Only generate if we're within threshold of horizon
            if (lastInstanceDate > DateOnly.FromDateTime(DateTime.UtcNow)
                .AddMonths(horizonMonths)
                .AddDays(-generationThresholdDays))
            {
                continue;
            }

            var startDate = lastInstanceDate.AddDays(1);

            var dayOfWeek = (DayOfWeek)assignment.OrderSchedule.DayOfWeek;

            var newDates = _dateGenerator.GetDates(
               startDate,
                order.EndDate,
                order.RecurrencePattern ?? RecurrencePattern.Daily,
                dayOfWeek,
                horizonMonths
            );



            foreach (var date in newDates)
            {
                jobInstances.Add(new JobInstance
                {
                    ScheduleAssignmentId = assignment.Id,
                    ScheduledDate = date,
                    StartTime = assignment.OrderSchedule.StartTime,
                    EndTime = assignment.OrderSchedule.EndTime,
                    Status = JobInstanceStatus.Upcoming
                });
            }

        }

        await _jobInstanceRepository.AddRangeAsync(jobInstances);
    }
}