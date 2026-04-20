
using System.Text.Json;
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
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
        private readonly IUserRepository _userRepository;
        private readonly IPricingConfigurationRepository _pricingConfigRepo;
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

ICustomerRepository customerRepo,
IUserRepository userRepository,
IPricingConfigurationRepository pricingConfigRepo
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
                _userRepository = userRepository;
                _pricingConfigRepo = pricingConfigRepo;
        }


        public async Task<List<SessionDto>> GetJobInstancesByAssignmentAsync(int assignmentId)
        {

                return _mapper.Map<List<SessionDto>>(await _jobInstanceRepository.GetByAssignmentAsync(assignmentId));
        }

        public async Task<List<SessionDto>> GetJobInstancesByOrderAsync(int orderId)
        {
                return _mapper.Map<List<SessionDto>>(await _jobInstanceRepository.GetByOrderIdAsync(orderId));
        }

        public async Task<List<SessionDto>> GetJobInstancesByOrderAsync(int orderId, DateOnly? from, DateOnly? to)
        {
                return _mapper.Map<List<SessionDto>>(await _jobInstanceRepository.GetByOrderIdAsync(orderId, from, to));
        }

        public async Task<List<SessionDto>> GetJobInstancesByStudentAsync(int studentId)
        {
                var jobInstances = await _jobInstanceRepository.GetJobInstancesByStudentAsync(studentId);
                return _mapper.Map<List<SessionDto>>(jobInstances);
        }

        public async Task<List<SessionDto>> GetJobInstances()
        {
                var jobInstances = await _jobInstanceRepository.GetJobInstances();
                return _mapper.Map<List<SessionDto>>(jobInstances);
        }

        public async Task<List<SessionDto>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _jobInstanceRepository.GetSeniorCompletedJobInstances(seniorId);
                return _mapper.Map<List<SessionDto>>(jobInstances);
        }
        public async Task<List<SessionDto>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceRepository.GetStudentCompletedJobInstances(studentId);
                return _mapper.Map<List<SessionDto>>(jobInstances);
        }
        public async Task<List<SessionDto>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _jobInstanceRepository.GetStudentUpComingJobInstances(studentId);
                return _mapper.Map<List<SessionDto>>(jobInstances);
        }

        public async Task RemindStudentAsync(int jobInstanceId)
        {

                var instance = await _jobInstanceRepository.LoadJobInstanceWithIncludes(jobInstanceId, new SessionIncludeOptions
                {
                        Assignment = true,
                        AssignmentStudent = true
                });

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.Upcoming) return;

                if (instance.PaymentStatus != PaymentStatus.Paid) return;

                var culture = instance.ScheduleAssignment?.Student?.Contact?.LanguageCode ?? "en";

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

                        // Create pending reviews IMMEDIATELY so users can review right away
                        var studentId = instance.ScheduleAssignment!.StudentId;
                        var seniorReview = new Review
                        {
                                Type = ReviewType.SeniorToStudent,
                                SeniorId = instance.SeniorId,
                                SeniorFullName = instance.Senior.Contact.FullName,
                                StudentId = studentId,
                                StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                                JobInstanceId = jobInstanceId,
                                Rating = 0,
                                Comment = null,
                                RetryCount = 0,
                                MaxRetry = 2,
                                NextRetryAt = DateTime.UtcNow,
                                IsPending = true,
                                CreatedAt = DateTime.UtcNow,
                        };
                        await _reviewRepo.AddAsync(seniorReview);

                        var studentReview = new Review
                        {
                                Type = ReviewType.StudentToSenior,
                                SeniorId = instance.SeniorId,
                                SeniorFullName = instance.Senior.Contact.FullName,
                                StudentId = studentId,
                                StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                                JobInstanceId = jobInstanceId,
                                Rating = 0,
                                Comment = null,
                                RetryCount = 0,
                                MaxRetry = 2,
                                NextRetryAt = DateTime.UtcNow,
                                IsPending = true,
                                CreatedAt = DateTime.UtcNow,
                        };
                        await _reviewRepo.AddAsync(studentReview);

                        // Schedule push notification reminder (10 min later)
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

        /// <summary>
        /// Idempotent completion — ensures a session is marked completed and pending
        /// reviews exist.  Called by the frontend when the user wants to review but
        /// Hangfire hasn't processed the session yet.
        /// </summary>
        public async Task<bool> EnsureCompletedAsync(int jobInstanceId)
        {
                var instance = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);
                if (instance == null)
                        return false;

                // Already completed — make sure pending reviews exist, re-create if missing
                if (instance.Status == JobInstanceStatus.Completed)
                {
                        var existing = await _reviewRepo.GetPendingByJobInstanceAsync(jobInstanceId);
                        if (existing.Count > 0)
                                return true;

                        // No pending reviews — re-create them if assignment exists
                        if (instance.ScheduleAssignmentId == null || instance.ScheduleAssignment?.Student == null)
                                return false;

                        var reStudentId = instance.ScheduleAssignment!.StudentId;

                        var reSeniorReview = new Review
                        {
                                Type = ReviewType.SeniorToStudent,
                                SeniorId = instance.SeniorId,
                                SeniorFullName = instance.Senior.Contact.FullName,
                                StudentId = reStudentId,
                                StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                                JobInstanceId = jobInstanceId,
                                Rating = 0,
                                Comment = null,
                                RetryCount = 0,
                                MaxRetry = 2,
                                NextRetryAt = DateTime.UtcNow,
                                IsPending = true,
                                CreatedAt = DateTime.UtcNow,
                        };
                        await _reviewRepo.AddAsync(reSeniorReview);

                        var reStudentReview = new Review
                        {
                                Type = ReviewType.StudentToSenior,
                                SeniorId = instance.SeniorId,
                                SeniorFullName = instance.Senior.Contact.FullName,
                                StudentId = reStudentId,
                                StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                                JobInstanceId = jobInstanceId,
                                Rating = 0,
                                Comment = null,
                                RetryCount = 0,
                                MaxRetry = 2,
                                NextRetryAt = DateTime.UtcNow,
                                IsPending = true,
                                CreatedAt = DateTime.UtcNow,
                        };
                        await _reviewRepo.AddAsync(reStudentReview);

                        return true;
                }

                // Only allow if time has actually passed
                var now = DateTime.UtcNow;
                var endUtc = instance.ScheduledDate.ToDateTime(instance.EndTime, DateTimeKind.Utc);
                if (now < endUtc)
                        return false;

                // Must have an assignment
                if (instance.ScheduleAssignmentId == null)
                        return false;

                // Status must be Upcoming or InProgress (not Cancelled/Rescheduled)
                if (instance.Status != JobInstanceStatus.Upcoming &&
                    instance.Status != JobInstanceStatus.InProgress)
                        return false;

                var assignment = await _assignmentRepository.LoadAssignmentWithIncludes(
                    instance.ScheduleAssignmentId.Value,
                    new AssignmentIncludeOptions
                    {
                            IncludeStudent = true,
                            IncludeStudentContracts = true
                    });

                if (assignment == null)
                        return false;

                var studentActiveContract = assignment.Student.ActiveContract;
                if (studentActiveContract != null)
                        instance.ContractId = studentActiveContract.Id;

                instance.Status = JobInstanceStatus.Completed;
                await _jobInstanceRepository.UpdateAsync(instance);

                await _statusMaintenanceService.MaintainOrderStatuses(instance.OrderId);

                // Create pending reviews (same logic as UpdateToCompletedAsync)
                var studentId = instance.ScheduleAssignment!.StudentId;

                var seniorReview = new Review
                {
                        Type = ReviewType.SeniorToStudent,
                        SeniorId = instance.SeniorId,
                        SeniorFullName = instance.Senior.Contact.FullName,
                        StudentId = studentId,
                        StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                        JobInstanceId = jobInstanceId,
                        Rating = 0,
                        Comment = null,
                        RetryCount = 0,
                        MaxRetry = 2,
                        NextRetryAt = DateTime.UtcNow,
                        IsPending = true,
                        CreatedAt = DateTime.UtcNow,
                };
                await _reviewRepo.AddAsync(seniorReview);

                var studentReview = new Review
                {
                        Type = ReviewType.StudentToSenior,
                        SeniorId = instance.SeniorId,
                        SeniorFullName = instance.Senior.Contact.FullName,
                        StudentId = studentId,
                        StudentFullName = instance.ScheduleAssignment.Student.Contact.FullName,
                        JobInstanceId = jobInstanceId,
                        Rating = 0,
                        Comment = null,
                        RetryCount = 0,
                        MaxRetry = 2,
                        NextRetryAt = DateTime.UtcNow,
                        IsPending = true,
                        CreatedAt = DateTime.UtcNow,
                };
                await _reviewRepo.AddAsync(studentReview);

                return true;
        }


        public async Task RequestJobReviewAsync(int jobInstanceId)
        {
                var instance = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.Completed) return;

                var customerId = instance.Senior.CustomerId;
                var customer = await _customerRepo.GetByIdAsync(customerId);
                var customerCulture = customer?.Contact.LanguageCode ?? "en";
                var studentId = instance.ScheduleAssignment!.StudentId;

                // Only send push notification reminders for still-pending reviews
                var pendingReviews = await _reviewRepo.GetPendingByJobInstanceAsync(jobInstanceId);

                var seniorPending = pendingReviews.FirstOrDefault(r => r.Type == ReviewType.SeniorToStudent && r.IsPending);
                if (seniorPending != null)
                {
                        var seniorNotification = _notificationFactory.ReviewRequestNotification(customerId, seniorPending, instance, customerCulture);
                        await _notificationService.SendNotificationAsync(customerId, seniorNotification);
                }

                var studentPending = pendingReviews.FirstOrDefault(r => r.Type == ReviewType.StudentToSenior && r.IsPending);
                if (studentPending != null)
                {
                        var studentNotification = _notificationFactory.ReviewRequestNotification(studentId, studentPending, instance, "hr");
                        await _notificationService.SendNotificationAsync(studentId, studentNotification);
                }
        }



        /// can reschdule 
        /// can change assignmened student

        public async Task<SessionDto?> ManageJobInstance(
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

                // Check if the student is actually changing
                int? currentStudentId = originalInstance.ScheduleAssignment?.StudentId;
                bool isStudentChanging = preferedStudentId.HasValue
                    && preferedStudentId.Value != currentStudentId;

                JobInstance? resultInstance = null;

                if (isReschedule && !isStudentChanging)
                {
                        // Simple in-place date/time update — no need for new session
                        resultInstance = await HandleSimpleReschedule(
                            originalInstance,
                            newDate, newStartTime, newEndTime);
                }
                else if (isReschedule && isStudentChanging)
                {
                        // Date/time change WITH student change — full reschedule flow
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

                return _mapper.Map<SessionDto>(resultInstance);

        }

        private async Task NotifyUsersJobInstanceRescheduled(
            JobInstance originalJobInstance,
            JobInstance updatedJobInstance,
            bool notifyAssignedStudent)
        {
                try
                {
                        var seniorCulture = originalJobInstance.Senior.Contact.LanguageCode ?? "hr";
                        var seniorNotification = _notificationFactory.JobRescheduledNotification(
                            receiverUserId: originalJobInstance.Senior.CustomerId,
                            originalJobInstance: originalJobInstance,
                            updatedJobInstance: updatedJobInstance,
                            culture: seniorCulture);
                        await _notificationService.StoreAndNotifyAsync(seniorNotification);

                        if (!notifyAssignedStudent || originalJobInstance.ScheduleAssignment?.Student == null)
                        {
                                return;
                        }

                        var student = originalJobInstance.ScheduleAssignment.Student;
                        var studentCulture = student.Contact.LanguageCode ?? "hr";
                        var studentNotification = _notificationFactory.JobRescheduledNotification(
                            receiverUserId: student.UserId,
                            originalJobInstance: originalJobInstance,
                            updatedJobInstance: updatedJobInstance,
                            culture: studentCulture);
                        await _notificationService.StoreAndNotifyAsync(studentNotification);
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to notify users about rescheduled job. OriginalJobInstanceId={JobInstanceId}", originalJobInstance.Id);
                }
        }

        private async Task NotifyAdminsJobInstanceRescheduled(JobInstance originalJobInstance, JobInstance updatedJobInstance)
        {
                try
                {
                        var adminIds = await _userRepository.GetAdminIdsAsync();
                        await _notificationService.StoreAndNotifyAdminsAsync(
                                adminIds,
                                adminId => _notificationFactory.JobRescheduledNotification(
                                    receiverUserId: adminId,
                                    originalJobInstance: originalJobInstance,
                                    updatedJobInstance: updatedJobInstance,
                                    culture: "hr"));
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to notify admins about rescheduled job. OriginalJobInstanceId={JobInstanceId}", originalJobInstance.Id);
                }
        }

        private async Task<JobInstance> HandleSimpleReschedule(
            JobInstance originalInstance,
            DateOnly? newDate,
            TimeOnly? newStartTime,
            TimeOnly? newEndTime)
        {
                DateOnly effectiveDate = newDate ?? originalInstance.ScheduledDate;
                TimeOnly effectiveStartTime = newStartTime ?? originalInstance.StartTime;
                TimeOnly effectiveEndTime = newEndTime ?? originalInstance.EndTime;

                bool isChanged =
                    effectiveDate != originalInstance.ScheduledDate ||
                    effectiveStartTime != originalInstance.StartTime ||
                    effectiveEndTime != originalInstance.EndTime;

                if (!isChanged)
                {
                        throw new InvalidOperationException("No changes were made to the job instance.");
                }

                // Check for senior-side conflict: does this senior already
                // have another active session overlapping the new date/time?
                var sessionsOnDate = await _jobInstanceRepository.GetByDateAsync(effectiveDate);
                var seniorConflict = sessionsOnDate.Any(j =>
                    j.Id != originalInstance.Id
                    && j.SeniorId == originalInstance.SeniorId
                    && j.Status != JobInstanceStatus.Cancelled
                    && j.Status != JobInstanceStatus.Rescheduled
                    && j.StartTime < effectiveEndTime
                    && j.EndTime > effectiveStartTime);

                if (seniorConflict)
                {
                        throw new InvalidOperationException(
                            $"Senior already has another session on {effectiveDate} that overlaps with {effectiveStartTime}-{effectiveEndTime}.");
                }

                var previousJobState = new JobInstance
                {
                        Id = originalInstance.Id,
                        SeniorId = originalInstance.SeniorId,
                        Senior = originalInstance.Senior,
                        OrderId = originalInstance.OrderId,
                        OrderScheduleId = originalInstance.OrderScheduleId,
                        ScheduleAssignment = originalInstance.ScheduleAssignment,
                        ScheduledDate = originalInstance.ScheduledDate,
                        StartTime = originalInstance.StartTime,
                        EndTime = originalInstance.EndTime,
                };

                originalInstance.ScheduledDate = effectiveDate;
                originalInstance.StartTime = effectiveStartTime;
                originalInstance.EndTime = effectiveEndTime;
                originalInstance.IsRescheduleVariant = true;
                originalInstance.RescheduledAt = DateTime.UtcNow;

                await _jobInstanceRepository.UpdateAsync(originalInstance);
                await NotifyUsersJobInstanceRescheduled(previousJobState, originalInstance, notifyAssignedStudent: true);
                await NotifyAdminsJobInstanceRescheduled(previousJobState, originalInstance);

                return originalInstance;
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

                // Check for senior-side conflict on the new date/time
                var sessionsOnDate = await _jobInstanceRepository.GetByDateAsync(effectiveDate);
                var seniorConflict = sessionsOnDate.Any(j =>
                    j.Id != originalInstance.Id
                    && j.SeniorId == originalInstance.SeniorId
                    && j.Status != JobInstanceStatus.Cancelled
                    && j.Status != JobInstanceStatus.Rescheduled
                    && j.StartTime < effectiveEndTime
                    && j.EndTime > effectiveStartTime);

                if (seniorConflict)
                {
                        throw new InvalidOperationException(
                            $"Senior already has another session on {effectiveDate} that overlaps with {effectiveStartTime}-{effectiveEndTime}.");
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
                        StudentHourlyRate = originalInstance.StudentHourlyRate,
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

                await NotifyUsersJobInstanceRescheduled(originalInstance, rescheduledInstance, notifyAssignedStudent: false);
                await NotifyAdminsJobInstanceRescheduled(originalInstance, rescheduledInstance);


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




        public async Task<SessionDto?> CancelJobInstance(int jobInstanceId, bool isAdmin = false, string callerRole = "")
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

                        // v2: Role-based cancel cutoff — read from PricingConfiguration
                        if (!isAdmin)
                        {
                                var config = await _pricingConfigRepo.GetByIdAsync(1);
                                var sessionStart = job.ScheduledDate.ToDateTime(job.StartTime);
                                var isSenior = string.Equals(callerRole, "Customer", StringComparison.OrdinalIgnoreCase);
                                var cutoffHours = isSenior
                                        ? (config?.SeniorCancelCutoffHours ?? 1)
                                        : (config?.StudentCancelCutoffHours ?? 6);

                                if (sessionStart <= DateTime.UtcNow.AddHours(cutoffHours))
                                {
                                        throw new DomainException(
                                                $"Cannot cancel session — it starts within {cutoffHours} hour(s)");
                                }
                        }

                        job.Status = JobInstanceStatus.Cancelled;

                        await _jobInstanceRepository.UpdateAsync(job);




                        await _statusMaintenanceService.MaintainOrderStatuses(job.OrderId);

                        await NotifyUsersJobInstanceCancelled(job);
                        await NotifyAdminsJobInstanceCancelled(job);

                        return _mapper.Map<SessionDto>(job);
                }
                catch (DomainException)
                {
                        throw;
                }
                catch (Exception ex)
                {
                        _logger.LogError("❌ [failed] Cancell  JobInstance {jobInstanceId}. {error}", jobInstanceId, ex);
                        return null;
                }


        }

        public async Task<SessionDto?> ReactivateJobInstance(int jobInstanceId)
        {
                try
                {
                        var job = await _jobInstanceRepository.GetByIdAsync(jobInstanceId);

                        if (job == null)
                        {
                                throw new ArgumentException($"Job instance {jobInstanceId} not found");
                        }

                        if (job.Status == JobInstanceStatus.Cancelled)
                        {
                                // Simple reactivation: Cancelled → Upcoming
                                job.Status = JobInstanceStatus.Upcoming;
                        }
                        else if (job.Status == JobInstanceStatus.Rescheduled)
                        {
                                // Revert reschedule: cancel the replacement session, restore original
                                if (job.RescheduledToId.HasValue)
                                {
                                        var replacement = await _jobInstanceRepository.GetByIdAsync(job.RescheduledToId.Value);
                                        if (replacement != null && replacement.Status == JobInstanceStatus.Upcoming)
                                        {
                                                replacement.Status = JobInstanceStatus.Cancelled;
                                                replacement.NeedsSubstitute = true; // hide from normal queries
                                                await _jobInstanceRepository.UpdateAsync(replacement);
                                        }
                                }

                                job.Status = JobInstanceStatus.Upcoming;
                                job.RescheduledToId = null;
                                job.RescheduledAt = null;
                                job.NeedsSubstitute = false;
                        }
                        else
                        {
                                throw new ArgumentException($"Cannot reactivate job instance {jobInstanceId} with status {job.Status}");
                        }

                        await _jobInstanceRepository.UpdateAsync(job);

                        // Skip job instance auto-cancellation during reactivation:
                        // The admin explicitly reactivated this session, so the past-date
                        // auto-cancel logic in JobInstanceStatusUpdater must not override it.
                        await _statusMaintenanceService.MaintainOrderStatuses(job.OrderId, skipJobInstanceUpdate: true);

                        return _mapper.Map<SessionDto>(job);
                }
                catch (Exception ex)
                {
                        _logger.LogError("❌ [failed] Reactivate JobInstance {jobInstanceId}. {error}", jobInstanceId, ex);
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

        private async Task NotifyAdminsJobInstanceCancelled(JobInstance jobInstance)
        {
                try
                {
                        var adminIds = await _userRepository.GetAdminIdsAsync();
                        await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                                adminId => _notificationFactory.JobCancelledNotification(adminId, jobInstance, "hr"));
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to notify admins about job cancellation. JobInstanceId={JobInstanceId}", jobInstance.Id);
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

        /// <inheritdoc/>
        public async Task StampCanCancelAsync(IEnumerable<SessionDto> sessions, string callerRole)
        {
                var config = await _pricingConfigRepo.GetByIdAsync(1);
                var isAdmin = string.Equals(callerRole, "Admin", StringComparison.OrdinalIgnoreCase);
                var isSenior = string.Equals(callerRole, "Customer", StringComparison.OrdinalIgnoreCase);
                var cutoffHours = isSenior
                        ? (config?.SeniorCancelCutoffHours ?? 1)
                        : (config?.StudentCancelCutoffHours ?? 6);
                var now = DateTime.UtcNow;

                foreach (var s in sessions)
                {
                        if (s.Status != Domain.Enums.JobInstanceStatus.Upcoming)
                        {
                                s.CanCancel = false;
                                continue;
                        }
                        if (isAdmin)
                        {
                                s.CanCancel = true;
                                continue;
                        }
                        var start = s.ScheduledDate.ToDateTime(s.StartTime);
                        s.CanCancel = start > now.AddHours(cutoffHours);
                }
        }
}
