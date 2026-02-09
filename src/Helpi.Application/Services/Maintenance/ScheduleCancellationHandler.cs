using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services.Maintenance;

public class ScheduleCancellationHandler
{
    private readonly IJobRequestRepository _jobRequestRepository;
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IOrderScheduleRepository _scheduleRepository;
    private readonly ILogger<ScheduleCancellationHandler> _logger;

    public ScheduleCancellationHandler(
        IJobRequestRepository jobRequestRepository,
        IJobInstanceRepository jobInstanceRepository,
        IOrderScheduleRepository scheduleRepository,
        ILogger<ScheduleCancellationHandler> logger
    )
    {
        _jobRequestRepository = jobRequestRepository;
        _jobInstanceRepository = jobInstanceRepository;
        _scheduleRepository = scheduleRepository;
        _logger = logger;
    }

    public async Task CancelScheduleAsync(OrderSchedule schedule, string cancellationReason)
    {
        schedule.IsCancelled = true;
        schedule.CancellationReason = cancellationReason;
        schedule.AllowAutoScheduling = false;
        schedule.AutoScheduleDisableReason = AutoScheduleDisableReason.admin;

        _logger.LogDebug("Marked schedule {ScheduleId} as cancelled", schedule.Id);

        if (!schedule.Assignments.Any())
        {
            _jobRequestRepository.MarkForDeleteRange(schedule.JobRequests);
            _scheduleRepository.MarkForsDelete(schedule);
            _logger.LogDebug("Deleted schedule {ScheduleId} with no assignments", schedule.Id);
            return;
        }

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var futureJobInstances = await _jobInstanceRepository.GetFromDateForScheduleAsync(now, schedule.Id);
        var pendingJobs = futureJobInstances.Where(j => j.Status != JobInstanceStatus.Completed);

        _jobInstanceRepository.MarkForDeleteRange(pendingJobs);

        var pendingJobRequests = schedule.JobRequests.Where(j => j.Status == JobRequestStatus.Pending);
        foreach (var request in pendingJobRequests)
        {
            request.Status = JobRequestStatus.Cancelled;
        }

        _logger.LogDebug("Cancelled future jobs for schedule {ScheduleId}", schedule.Id);
    }
}
