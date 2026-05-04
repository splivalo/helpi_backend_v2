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
            var now0 = DateOnly.FromDateTime(DateTime.UtcNow);
            var futureJobs0 = await _jobInstanceRepository.GetFromDateForScheduleAsync(now0, schedule.Id);
            var pendingJobs0 = futureJobs0.Where(j => j.Status != JobInstanceStatus.Completed).ToList();
            foreach (var ji in pendingJobs0)
                ji.Status = JobInstanceStatus.Cancelled;

            // Mark schedule itself as cancelled (do not hard-delete — FK constraints may prevent it
            // and IsCancelled=false is what causes the app to keep displaying 'Planirano' sessions)
            schedule.IsCancelled = true;
            schedule.CancellationReason = cancellationReason;
            schedule.AllowAutoScheduling = false;
            schedule.AutoScheduleDisableReason = AutoScheduleDisableReason.admin;

            var unassignedJobRequests = schedule.JobRequests.Where(j => j.Status == JobRequestStatus.Pending);
            foreach (var req in unassignedJobRequests)
                req.Status = JobRequestStatus.Cancelled;

            _logger.LogDebug("Cancelled {JobCount} job instances and schedule {ScheduleId} (no assignments)",
                pendingJobs0.Count, schedule.Id);
            return;
        }

        // Terminate all active/pending assignments on this schedule
        var activeAssignments = schedule.Assignments
            .Where(a => a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.PendingAcceptance)
            .ToList();

        foreach (var assignment in activeAssignments)
        {
            assignment.Status = AssignmentStatus.Terminated;
            assignment.TerminationReason = TerminationReason.OrderCancelled;
            assignment.TerminatedAt = DateTime.UtcNow;
            _logger.LogDebug("Terminated assignment {AssignmentId} (Student {StudentId}) on schedule {ScheduleId}",
                assignment.Id, assignment.StudentId, schedule.Id);
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

        _logger.LogDebug("Cancelled future jobs and terminated {AssignmentCount} assignment(s) for schedule {ScheduleId}",
            activeAssignments.Count, schedule.Id);
    }
}
