
using System.Text.Json;
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class JobInstanceService : IJobInstanceService
{
        private readonly IJobInstanceRepository _jobInstanceRepository;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly CompletionStatusService _completionStatusService;
        private readonly IReassignmentService _reassignmentService;
        private readonly IScheduleAssignmentRepository _assignmentRepository;

        private readonly IHangfireService _hangfireService;
        private readonly ILogger<JobInstanceService> _logger;
        public JobInstanceService(
                IJobInstanceRepository repository,
                IMapper mapper,
                INotificationService notificationService,
                 CompletionStatusService completionStatusService,
                  IHangfireService hangfireService,
                  IReassignmentService reassignmentService,
                  IScheduleAssignmentRepository assignmentRepository,
                   ILogger<JobInstanceService> logger
                )
        {
                _jobInstanceRepository = repository;
                _mapper = mapper;
                _notificationService = notificationService;
                _completionStatusService = completionStatusService;
                _hangfireService = hangfireService;
                _reassignmentService = reassignmentService;
                _assignmentRepository = assignmentRepository;
                _logger = logger;
        }


        public async Task<List<JobInstanceDto>> GetJobInstancesByAssignmentAsync(int assignmentId)
        {

                return _mapper.Map<List<JobInstanceDto>>(await _jobInstanceRepository.GetByAssignmentAsync(assignmentId));
        }

        public async Task<List<JobInstanceDto>> GetJobInstancesByStudentAsync(int studentId)
        {
                var jobInstances = await _jobInstanceRepository.GetJobInstancesByStudentAsync(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }

        public async Task<List<JobInstanceDto>> GetJobInstances()
        {
                var jobInstances = await _jobInstanceRepository.GetJobInstances();
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }

        public async Task<List<JobInstanceDto>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _jobInstanceRepository.GetSeniorCompletedJobInstances(seniorId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }
        public async Task<List<JobInstanceDto>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceRepository.GetStudentCompletedJobInstances(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }
        public async Task<List<JobInstanceDto>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceRepository.GetStudentUpComingJobInstances(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }


        public async Task UpdateToInProgressAsync(int jobInstanceId)
        {

                var instance = await _jobInstanceRepository.UpdateToInProgressAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.InProgress) return;


                var assignedStudentId = instance.Assignment.StudentId;
                await _notificationService.SendJobStartedNotificationAsync(assignedStudentId, instance);

                var customerId = instance.Senior.CustomerId;
                await _notificationService.SendJobCompletedNotificationAsync(customerId, instance);

        }

        public async Task UpdateToCompletedAsync(int jobInstanceId)
        {
                try
                {
                        var instance = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);

                        if (instance == null)
                        {
                                LogJobInstanceNotFound(jobInstanceId);
                                return;
                        }

                        if (instance.Status != JobInstanceStatus.InProgress)
                        {
                                LogJobInstanceNotInProgress(jobInstanceId, instance.Status);
                                return;
                        }

                        var assignment = await _assignmentRepository.LoadAssignmentWithIncludes(
                            instance.ScheduleAssignmentId,
                            new AssignmentIncludeOptions
                            {
                                    IncludeStudent = true,
                                    IncludeStudentContracts = true
                            });

                        if (assignment == null)
                        {
                                LogAssignmentNotFound(instance.ScheduleAssignmentId, jobInstanceId);
                                return;
                        }

                        var studentActiveContract = assignment.Student.ActiveContract;
                        if (studentActiveContract == null)
                        {
                                LogStudentNoActiveContract(assignment.Student.UserId, jobInstanceId);
                                return;
                        }

                        // Apply completion updates
                        instance.ContractId = studentActiveContract.Id;
                        instance.Status = JobInstanceStatus.Completed;

                        await _jobInstanceRepository.UpdateAsync(instance);

                        if (instance.Status != JobInstanceStatus.Completed)
                        {
                                LogCompletionUpdateFailed(jobInstanceId);
                                return;
                        }

                        // Notifications
                        await _notificationService.SendJobCompletedNotificationAsync(instance.Assignment.Id, instance);

                        await _notificationService.SendJobCompletedNotificationAsync(instance.Senior.CustomerId, instance);

                        // Post-completion processing
                        await _completionStatusService.ProcessCompletionStatuses(instance.OrderId);

                        // Schedule review request
                        var reviewRequestTime = DateTime.UtcNow.AddMinutes(5);
                        _hangfireService.Schedule<IJobInstanceService>(
                            s => s.RequestJobReviewAsync(instance.Id),
                            reviewRequestTime
                        );
                        LogReviewScheduled(jobInstanceId, reviewRequestTime);
                }
                catch (Exception ex)
                {
                        LogException(jobInstanceId, ex);
                }
        }




        public async Task RequestJobReviewAsync(int jobInstanceId)
        {
                var instance = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.Completed) return;

                var customerId = instance.Senior.CustomerId;

                var notification = new HNotification
                {
                        RecieverUserId = customerId,
                        Title = "Review",
                        Body = "How was your expirence? ",
                        Type = NotificationType.ReviewRequest,
                        Payload = JsonSerializer.Serialize(new
                        {
                                RecieverUserId = customerId,
                                JobInstanceId = jobInstanceId,
                                SeniorId = instance.SeniorId,
                                SeniorFullName = instance.Senior.Contact.FullName,
                                StudentId = instance.Assignment.StudentId,
                                StudentFullName = instance.Assignment.Student.Contact.FullName,
                        })
                };


                await _notificationService.SendPushNotificationAsync(customerId, notification);
        }


        /// can reschdule 
        /// can change assignmened student

        public async Task<JobInstanceDto?> ManageJobInstance(
            int jobInstanceId,
            DateOnly? newDate,
            TimeOnly? newStartTime,
            TimeOnly? newEndTime,
            string reason,
            int? preferedStudentId,
            bool reassignStudent = true,
            int requestedByUserId = 1)
        {
                var originalInstance = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);

                if (originalInstance == null)
                {
                        throw new ArgumentException($"Job instance {jobInstanceId} not found");
                }

                if (originalInstance.Status == JobInstanceStatus.Rescheduled)
                {
                        throw new ArgumentException($"Job instance {jobInstanceId} was  already rescheduled before");
                }

                var noReschedule = new[] {
                        JobInstanceStatus.Completed,
                        JobInstanceStatus.Cancelled,
                        JobInstanceStatus.InProgress
                };

                if (noReschedule.Contains(originalInstance.Status))
                {
                        throw new ArgumentException($"can not reschudel Job instance {jobInstanceId} with status  {originalInstance.Status}");
                }


                // Determine if this is a reschedule, reassignment, or both
                bool isReschedule = newDate.HasValue || newStartTime.HasValue || newEndTime.HasValue;


                JobInstance? resultInstance = null;

                if (isReschedule)
                {
                        //
                        resultInstance = await HandleReschedule(
                            originalInstance,
                            newDate, newStartTime, newEndTime,
                            reason,
                            preferedStudentId,
                            requestedByUserId);
                }
                else if (reassignStudent)
                {
                        // Handle reassignment only
                        resultInstance = await HandleReassignment(
                            originalInstance,
                            reason,
                            requestedByUserId,
                               preferedStudentId);
                }

                return _mapper.Map<JobInstanceDto>(resultInstance);

        }

        private async Task<JobInstance> HandleReschedule(
            JobInstance originalInstance,
            DateOnly? newDate,
            TimeOnly? newStartTime,
            TimeOnly? newEndTime,
            string reason,
            int? preferedStudentId,
            int requestedByUserId)
        {
                // Use original values if new ones not provided
                DateOnly effectiveDate = newDate ?? originalInstance.ScheduledDate;
                TimeOnly effectiveStartTime = newStartTime ?? originalInstance.StartTime;
                TimeOnly effectiveEndTime = newEndTime ?? originalInstance.EndTime;


                // ✅ Validation: at least one change must exist
                bool isChanged =
                    effectiveDate != originalInstance.ScheduledDate ||
                    effectiveStartTime != originalInstance.StartTime ||
                    effectiveEndTime != originalInstance.EndTime;

                if (!isChanged)
                {
                        throw new InvalidOperationException("Reschedule must change at least one of Date, StartTime, or EndTime.");
                }

                // Create a new instance with the updated times
                var rescheduledInstance = new JobInstance
                {
                        // Copy properties from original instance
                        ScheduleAssignmentId = originalInstance.ScheduleAssignmentId,
                        SeniorId = originalInstance.SeniorId,
                        CustomerId = originalInstance.CustomerId,
                        OrderId = originalInstance.OrderId,
                        OrderScheduleId = originalInstance.OrderScheduleId,
                        ContractId = originalInstance.ContractId,

                        // Use new times
                        ScheduledDate = effectiveDate,
                        StartTime = effectiveStartTime,
                        EndTime = effectiveEndTime,

                        // Rescheduling metadata
                        IsRescheduleVariant = true,
                        RescheduledFromId = originalInstance.Id,
                        RescheduledAt = DateTime.UtcNow,
                        RescheduleReason = reason,

                        // Initial status
                        Status = JobInstanceStatus.Upcoming,
                        NeedsSubstitute = true,

                        // Copy financial information
                        HourlyRate = originalInstance.HourlyRate,
                        CompanyPercentage = originalInstance.CompanyPercentage,
                        ServiceProviderPercentage = originalInstance.ServiceProviderPercentage
                };

                await _jobInstanceRepository.AddAsync(rescheduledInstance);

                // Mark original instance as rescheduled
                originalInstance.RescheduledToId = rescheduledInstance.Id;
                originalInstance.Status = JobInstanceStatus.Rescheduled;
                originalInstance.RescheduledAt = DateTime.UtcNow;
                await _jobInstanceRepository.UpdateAsync(originalInstance);



                // Initiate reassignment with preferred student (if specified)
                await _reassignmentService.InitiateReassignment(
                        ReassignmentType.OneDaySubstitution,
                        ReassignmentTrigger.AdminIntervention,
                        "requested by admin",
                        requestedByUserId,
                        rescheduledInstance.Id,
                        null,
                        preferedStudentId
                    );


                return rescheduledInstance;
        }

        private async Task<JobInstance> HandleReassignment(
            JobInstance originalInstance,
            string reason,
            int requestedByUserId,
             int? preferedStudentId)
        {
                // For reassignment only, we work with the existing instance
                originalInstance.NeedsSubstitute = true;
                originalInstance.RescheduleReason = reason;
                originalInstance.RescheduledAt = DateTime.UtcNow;
                await _jobInstanceRepository.UpdateAsync(originalInstance);

                // Initiate reassignment with preferred student
                await _reassignmentService.InitiateReassignment(
                      ReassignmentType.OneDaySubstitution,
                        ReassignmentTrigger.AdminIntervention,
                        "requested by admin",
                        requestedByUserId,
                        originalInstance.Id,
                        null,
                        preferedStudentId);

                return originalInstance;
        }



        private async Task NotifyReschedule(int studentId, JobInstance originalInstance, JobInstance rescheduledInstance, string reason)
        {
                var notification = new HNotification
                {
                        RecieverUserId = studentId,
                        Title = "Schedule Changed",
                        Body = $"Your job on {originalInstance.ScheduledDate} has been rescheduled to {rescheduledInstance.ScheduledDate} at {rescheduledInstance.StartTime}",
                        Type = NotificationType.ScheduleChange,
                        Payload = JsonSerializer.Serialize(new
                        {
                                OriginalInstanceId = originalInstance.Id,
                                NewInstanceId = rescheduledInstance.Id,
                                NewDate = rescheduledInstance.ScheduledDate,
                                NewStartTime = rescheduledInstance.StartTime,
                                Reason = reason
                        })
                };

                await _notificationService.SendPushNotificationAsync(studentId, notification);
        }


        #region Logging helpers

        private void LogJobInstanceNotFound(int jobInstanceId) =>
            _logger.LogWarning("⚠️ JobInstance {jobInstanceId} not found. Skipping completion.", jobInstanceId);

        private void LogJobInstanceNotInProgress(int jobInstanceId, JobInstanceStatus status) =>
            _logger.LogInformation("ℹ️ JobInstance {jobInstanceId} is not in progress (Status: {status}). No update.", jobInstanceId, status);

        private void LogAssignmentNotFound(int assignmentId, int jobInstanceId) =>
            _logger.LogWarning("⚠️ Assignment {assignmentId} not found for JobInstance {jobInstanceId}.", assignmentId, jobInstanceId);

        private void LogStudentNoActiveContract(int studentId, int jobInstanceId) =>
            _logger.LogWarning("⚠️ Student {studentId} has no active contract. JobInstance {jobInstanceId} cannot be completed.", studentId, jobInstanceId);

        private void LogCompletionUpdateFailed(int jobInstanceId) =>
            _logger.LogError("❌ Failed to update JobInstance {jobInstanceId} to Completed.", jobInstanceId);



        private void LogReviewScheduled(int jobInstanceId, DateTime reviewTime) =>
            _logger.LogInformation("📅 Scheduled review request for JobInstance {jobInstanceId} at {reviewTime}.", jobInstanceId, reviewTime);

        private void LogException(int jobInstanceId, Exception ex) =>
            _logger.LogError(ex, "❌ Exception while updating JobInstance {jobInstanceId} to completed.", jobInstanceId);

        #endregion
}
