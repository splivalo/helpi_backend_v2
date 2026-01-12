
using System.Text.Json;
using AutoMapper;
using Helpi.Application.Common.Interfaces;
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
        private readonly ICustomerRepository _customerRepo;
        private readonly INotificationFactory _notificationFactory;
        private readonly OrderStatusMaintenanceService _statusMaintenanceService;
        private readonly IReassignmentService _reassignmentService;
        private readonly IReviewRepository _reviewRepo;
        private readonly IScheduleAssignmentRepository _assignmentRepository;

        private readonly IHangfireService _hangfireService;
        private readonly ILogger<JobInstanceService> _logger;
        public JobInstanceService(
                IJobInstanceRepository repository,
                IMapper mapper,
                INotificationService notificationService,
INotificationFactory notificationFactory,
                 OrderStatusMaintenanceService statusMaintenanceService,
                  IHangfireService hangfireService,
                  IReassignmentService reassignmentService,
                  IReviewRepository reviewRepo,
                  IScheduleAssignmentRepository assignmentRepository,
                   ILogger<JobInstanceService> logger,

ICustomerRepository customerRepo
                )
        {
                _jobInstanceRepository = repository;
                _mapper = mapper;
                _notificationService = notificationService;
                _notificationFactory = notificationFactory;
                _statusMaintenanceService = statusMaintenanceService;
                _hangfireService = hangfireService;
                _reassignmentService = reassignmentService;
                _reviewRepo = reviewRepo;
                _assignmentRepository = assignmentRepository;
                _logger = logger;
                _customerRepo = customerRepo;
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

        public async Task RemindStudentAsync(int jobInstanceId)
        {

                var instance = await _jobInstanceRepository.LoadJobInstanceWithIncludes(jobInstanceId, new JobInstanceIncludeOptions
                {
                        Assignment = true,
                        AssignmentStudent = true
                });

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.Upcoming) return;

                if (instance.PaymentStatus != PaymentStatus.Paid) return;

                var culture = instance?.ScheduleAssignment?.Student.Contact.LanguageCode ?? "en";

                var notification = _notificationFactory.CreateStudentJobReminderNotification(instance, culture);
                await _notificationService.SendNotificationAsync(notification.RecieverUserId, notification);


        }


        public async Task UpdateToInProgressAsync(int jobInstanceId)
        {

                var instance = await _jobInstanceRepository.UpdateToInProgressAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.InProgress) return;


                // var assignedStudentId = instance.ScheduleAssignment.StudentId;
                // await _notificationService.SendJobStartedNotificationAsync(assignedStudentId, instance);

                // var customerId = instance.Senior.CustomerId;
                // await _notificationService.SendJobCompletedNotificationAsync(customerId, instance);

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

                        if (instance.ScheduleAssignmentId == null)
                        {
                                return;
                        }


                        var assignment = await _assignmentRepository.LoadAssignmentWithIncludes(
                            instance.ScheduleAssignmentId.Value,
                            new AssignmentIncludeOptions
                            {
                                    IncludeStudent = true,
                                    IncludeStudentContracts = true
                            });

                        if (assignment == null)
                        {
                                LogAssignmentNotFound(instance.ScheduleAssignmentId.Value, jobInstanceId);
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
                        // await _notificationService.SendJobCompletedNotificationAsync(
                        //         instance.ScheduleAssignment.StudentId, instance);

                        // await _notificationService.SendJobCompletedNotificationAsync(
                        //         instance.Senior.CustomerId, instance);

                        // Post-completion processing
                        await _statusMaintenanceService.MaintainOrderStatuses(instance.OrderId);

                        // Schedule review request
                        var reviewRequestTime = DateTime.UtcNow.AddMinutes(10);
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
                var cutomer = await _customerRepo.GetByIdAsync(customerId);
                var customerCulture = cutomer?.Contact.LanguageCode ?? "en";
                // Create the pending review in DB
                var review = new Review
                {
                        SeniorId = instance.SeniorId,
                        SeniorFullName = instance.Senior.Contact.FullName,
                        StudentId = instance.ScheduleAssignment!.StudentId,
                        StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                        JobInstanceId = jobInstanceId,
                        Rating = 0, // not rated yet
                        Comment = null,
                        RetryCount = 0,
                        MaxRetry = 2,
                        NextRetryAt = DateTime.UtcNow,
                        IsPending = true,
                        CreatedAt = DateTime.UtcNow,
                };

                await _reviewRepo.AddAsync(review);

                // Send notification to customer
                var notification = _notificationFactory.ReviewRequestNotification(customerId, review, instance, customerCulture);

                await _notificationService.SendNotificationAsync(customerId, notification);
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
                originalInstance.NeedsSubstitute = false;
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




        public async Task<JobInstanceDto?> CancelJobInstance(int jobInstanceId)
        {
                try
                {
                        var job = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);

                        if (job == null)
                        {
                                throw new ArgumentException($"Job instance {jobInstanceId} not found");
                        }

                        if (job.Status != JobInstanceStatus.Upcoming)
                        {
                                throw new ArgumentException($"can not cancel Job instance {jobInstanceId} with status  {job.Status}");
                        }

                        job.Status = JobInstanceStatus.Cancelled;

                        await _jobInstanceRepository.UpdateAsync(job);




                        await _statusMaintenanceService.MaintainOrderStatuses(job.OrderId);

                        await NotifyUsersJobInstanceCancelled(job);

                        return _mapper.Map<JobInstanceDto>(job);
                }
                catch (Exception ex)
                {
                        _logger.LogError("❌ [failed] Cancell  JobInstance {jobInstanceId}. {error}", jobInstanceId, ex);
                        return null;
                }


        }


        private async Task NotifyUsersJobInstanceCancelled(JobInstance jobInstance)
        {
                try
                {       // senior
                        var senior = jobInstance.Senior;
                        var customerId = senior.CustomerId;
                        var culture = senior.Contact.LanguageCode ?? "hr";
                        await NotifyJobInstanceCancelled(customerId, jobInstance, culture);

                        // student
                        if (jobInstance.ScheduleAssignment != null)
                        {
                                var student = jobInstance.ScheduleAssignment.Student;
                                var studentId = student.UserId;
                                var studentCulture = student.Contact.LanguageCode ?? "hr";
                                await NotifyJobInstanceCancelled(studentId, jobInstance, studentCulture);
                        }
                }
                catch (Exception)
                {
                        _logger.LogError("❌ Notify error JobInstance {jobInstanceId} .", jobInstance.Id);

                }


        }


        private async Task NotifyJobInstanceCancelled(int recieverId, JobInstance jobInstance, string culture)
        {
                try
                {
                        var noti = _notificationFactory.JobCancelledNotification(
                            recieverId,
                            jobInstance,
                            culture: culture

                            );

                        await _notificationService.SendNotificationAsync(recieverId, noti);
                }
                catch (Exception)
                {
                        _logger.LogError("❌ Notify error JobInstance {jobInstanceId} .", jobInstance.Id);

                }


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
