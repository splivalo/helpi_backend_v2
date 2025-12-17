using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class FailedMatchReinitiationService : IFailedMatchReinitiationService,
                                              IDomainEventHandler<ReinitiateAllFailedMatchesEvent>
{
    private readonly IOrderScheduleRepository _orderScheduleRepository;
    private readonly IReassignmentRecordRepository _reassignmentRecordRepository;
    private readonly IJobInstanceRepository _jobInstanceRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMatchingService _matchingService;
    private readonly IJobInstanceMatchingService _jobInstanceMatchingService;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<FailedMatchReinitiationService> _logger;

    public FailedMatchReinitiationService(
        IOrderScheduleRepository orderScheduleRepository,
        IReassignmentRecordRepository reassignmentRecordRepository,
        IJobInstanceRepository jobInstanceRepository,
        IOrderRepository orderRepository,
        IMatchingService matchingService,
        IJobInstanceMatchingService jobInstanceMatchingService,
        IStudentRepository studentRepository,
        ILogger<FailedMatchReinitiationService> logger)
    {
        _orderScheduleRepository = orderScheduleRepository;
        _reassignmentRecordRepository = reassignmentRecordRepository;
        _jobInstanceRepository = jobInstanceRepository;
        _orderRepository = orderRepository;
        _matchingService = matchingService;
        _jobInstanceMatchingService = jobInstanceMatchingService;
        _studentRepository = studentRepository;
        _logger = logger;
    }


    public async Task Handle(ReinitiateAllFailedMatchesEvent @event)
    {
        await ReinitiateAllFailedMatches();
    }

    public async Task ReinitiateAllFailedMatches()
    {
        _logger.LogInformation("♻️ Reinitiating all failed matches");

        // Reinitiate failed schedule matches
        var failedSchedules = await _orderScheduleRepository.GetFailedAutoSchedulingSchedules();
        foreach (var schedule in failedSchedules)
        {
            await ReinitiateScheduleMatching(schedule);
        }

        // Reinitiate failed reassignment matches
        var failedReassignments = await _reassignmentRecordRepository.GetRecordsForRematchingAttemptAsync();
        foreach (var reassignment in failedReassignments)
        {
            await ReinitiateReassignmentMatching(reassignment);
        }
    }

    public async Task ReinitiateMatchingForOrderSchedule(int orderScheduleId)
    {
        var schedule = await _orderScheduleRepository.GetByIdAsync(orderScheduleId);
        if (schedule != null && !schedule.IsCancelled)
        {
            await ReinitiateScheduleMatching(schedule);
        }
    }

    public async Task ReinitiateMatchingForReassignmentRecord(int reassignmentRecordId)
    {
        var reassignment = await _reassignmentRecordRepository.GetByIdAsync(reassignmentRecordId,
        new ReassignmentIncludeOptions { },
        asNoTracking: false);

        if (reassignment != null)
        {
            await ReinitiateReassignmentMatching(reassignment);
        }
    }



    private async Task ReinitiateScheduleMatching(OrderSchedule schedule)
    {
        try
        {
            // Reset the schedule for matching
            schedule.AllowAutoScheduling = true;
            schedule.AutoScheduleDisableReason = null;
            schedule.AutoScheduleAttemptCount = 0;

            await _orderScheduleRepository.UpdateAsync(schedule);

            // Start matching process
            await _matchingService.StartMatching(schedule.OrderId);

            _logger.LogInformation("✅ Reinitiated matching for schedule {ScheduleId}", schedule.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reinitiating matching for schedule {ScheduleId}", schedule.Id);
        }
    }

    private async Task ReinitiateReassignmentMatching(ReassignmentRecord reassignment)
    {
        try
        {
            // Reset the reassignment for matching
            reassignment.Status = ReassignmentStatus.InProgress;
            reassignment.AllowAutoScheduling = true;
            reassignment.AttemptCount = 0;

            await _reassignmentRecordRepository.UpdateAsync(reassignment);

            // Determine what type of reassignment this is and start the appropriate matching process
            if (reassignment.ReassignJobInstanceId.HasValue)
            {
                // Job instance reassignment
                await _jobInstanceMatchingService.StartJobInstanceMatchingAsync(
                    reassignment.ReassignJobInstanceId.Value, reassignment.Id);
            }
            else if (reassignment.ReassignAssignmentId.HasValue)
            {
                // Order schedule reassignment
                var schedule = await _orderScheduleRepository.GetByIdAsync(reassignment.OrderScheduleId);


                if (schedule != null)
                {
                    if (schedule.IsCancelled == false)
                    {
                        await _matchingService.StartMatching(reassignment.OrderId);
                    }

                }
            }

            _logger.LogInformation("✅ Reinitiated matching for reassignment {ReassignmentId}", reassignment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reinitiating matching for reassignment {ReassignmentId}", reassignment.Id);
        }
    }



}