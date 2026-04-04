using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class TravelBufferReconciliationService
{
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IReassignmentService _reassignmentService;
    private readonly ILogger<TravelBufferReconciliationService> _logger;

    public TravelBufferReconciliationService(
        IJobInstanceRepository jobInstanceRepository,
        IReassignmentService reassignmentService,
        ILogger<TravelBufferReconciliationService> logger)
    {
        _jobInstanceRepository = jobInstanceRepository;
        _reassignmentService = reassignmentService;
        _logger = logger;
    }

    public async Task<int> ReconcileAsync(int previousBufferMinutes, int newBufferMinutes)
    {
        if (newBufferMinutes <= previousBufferMinutes)
        {
            return 0;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcomingJobs = (await _jobInstanceRepository.GetJobInstances())
            .Where(job => job.Status == JobInstanceStatus.Upcoming)
            .Where(job => job.ScheduledDate >= today)
            .Where(job => job.NeedsSubstitute == false)
            .Where(job => job.ScheduleAssignment is { Status: AssignmentStatus.Accepted, IsJobInstanceSub: false })
            .ToList();

        var assignmentsToReassign = new HashSet<int>();

        foreach (var studentDayGroup in upcomingJobs
                     .Where(job => job.ScheduleAssignment?.StudentId != null)
                     .GroupBy(job => new
                     {
                         StudentId = job.ScheduleAssignment!.StudentId,
                         job.ScheduledDate,
                     }))
        {
            JobInstance? previousJob = null;

            foreach (var currentJob in studentDayGroup.OrderBy(job => job.StartTime))
            {
                if (previousJob == null)
                {
                    previousJob = currentJob;
                    continue;
                }

                var requiredStart = previousJob.EndTime.AddMinutes(newBufferMinutes);
                if (currentJob.StartTime < requiredStart)
                {
                    var conflictingAssignmentId = currentJob.ScheduleAssignmentId;
                    if (conflictingAssignmentId.HasValue)
                    {
                        assignmentsToReassign.Add(conflictingAssignmentId.Value);
                    }
                }
                else
                {
                    previousJob = currentJob;
                }
            }
        }

        var reassignedCount = 0;
        foreach (var assignmentId in assignmentsToReassign)
        {
            try
            {
                await _reassignmentService.ReassignAssignment(
                    assignmentId,
                    ReassignmentType.CompleteTakeover,
                    $"Travel buffer increased from {previousBufferMinutes} to {newBufferMinutes} minutes.");
                reassignedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to reconcile assignment {AssignmentId} after travel buffer change {PreviousBuffer}->{NewBuffer}.",
                    assignmentId,
                    previousBufferMinutes,
                    newBufferMinutes);
            }
        }

        _logger.LogInformation(
            "Travel buffer reconciliation finished. Previous={PreviousBuffer}, New={NewBuffer}, ReassignmentsStarted={ReassignedCount}",
            previousBufferMinutes,
            newBufferMinutes,
            reassignedCount);

        return reassignedCount;
    }
}