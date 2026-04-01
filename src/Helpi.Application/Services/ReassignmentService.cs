using System.Text.Json;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services
{
    public class ReassignmentService : IReassignmentService
    {
        private readonly IReassignmentRecordRepository _reassignmentRecordRepository;
        private readonly IJobInstanceRepository _jobInstanceRepository;
        private readonly IScheduleAssignmentRepository _scheduleAssignmentRepository;
        private readonly IOrderScheduleRepository _orderScheduleRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMatchingService _matchingService;
        private readonly IJobInstanceMatchingService _jobInstanceMatchingService;
        private readonly ILogger<ReassignmentService> _logger;
        private readonly INotificationService _notificationService;
        private readonly INotificationFactory _notificationFactory;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;

        public ReassignmentService(
            IReassignmentRecordRepository reassignmentRecordRepository,
            IJobInstanceRepository jobInstanceRepository,
            IScheduleAssignmentRepository scheduleAssignmentRepository,
            IOrderScheduleRepository orderScheduleRepository,
            IOrderRepository orderRepository,
             IMatchingService matchingService,
             IJobInstanceMatchingService jobInstanceMatchingService,
            ILogger<ReassignmentService> logger,
            INotificationService notificationService,
INotificationFactory notificationFactory,
            IStudentRepository studentRepository,
            IUserRepository userRepository
            )
        {
            _reassignmentRecordRepository = reassignmentRecordRepository;
            _jobInstanceRepository = jobInstanceRepository;
            _scheduleAssignmentRepository = scheduleAssignmentRepository;
            _orderScheduleRepository = orderScheduleRepository;
            _orderRepository = orderRepository;
            _matchingService = matchingService;
            _jobInstanceMatchingService = jobInstanceMatchingService;
            _logger = logger;
            _notificationService = notificationService;
            _notificationFactory = notificationFactory;
            _studentRepository = studentRepository;
            _userRepository = userRepository;
        }

        public async Task<ReassignmentRecord> InitiateReassignment(
            ReassignmentType reassignmentType,
            ReassignmentTrigger trigger,
            string reason,
            int requestedByUserId,
            int? jobInstanceId = null,
            int? scheduleAssignmentId = null,
            int? preferedStudentId = null
            )
        {
            _logger.LogInformation("🚀 Initiating reassignment: Type={Type}, Trigger={Trigger}, Reason={Reason}",
                reassignmentType, trigger, reason);

            // Validate that either jobInstanceId or scheduleAssignmentId is provided
            if (!jobInstanceId.HasValue && !scheduleAssignmentId.HasValue)
                throw new ArgumentException("Either jobInstanceId or scheduleAssignmentId must be provided");

            // Get the related entities to populate the reassignment record
            OrderSchedule? orderSchedule;
            Order? order;
            int? originalStudentId = null;
            int currentAssignmentId = 0;

            if (reassignmentType == ReassignmentType.OneDaySubstitution)
            {

                if (!jobInstanceId.HasValue)
                {
                    throw new ArgumentException(
                        "JobInstanceId  must be provided for ReassignmentTypeOneDaySubstitution");
                }


                var jobInstance = await _jobInstanceRepository.LoadJobInstanceWithIncludes(
                    jobInstanceId.Value,
                    new SessionIncludeOptions
                    {
                        Order = true,
                        OrderSchedule = true,
                        Assignment = true,
                        AssignmentOrderSchedule = true,
                        PrevAssignment = true,
                        PrevAssignmentOrderSchedule = true
                    });

                if (jobInstance == null)
                    throw new ArgumentException($"Job instance {jobInstanceId} not found");


                /// ScheduleAssignmentId can be null EG, schedule is being resassinged therefore the jobinstans has not assignment
                var knownAssignment = jobInstance.ScheduleAssignmentId.HasValue ?
                                                jobInstance.ScheduleAssignment
                                                : jobInstance.PrevAssignment;

                if (knownAssignment != null)
                {
                    orderSchedule = knownAssignment.OrderSchedule;
                    order = jobInstance.Order;
                    originalStudentId = knownAssignment.StudentId;
                    currentAssignmentId = knownAssignment.Id;
                }
                else
                {
                    // No assignment exists — session not yet assigned to a student
                    order = jobInstance.Order;
                    orderSchedule = jobInstance.OrderSchedule
                        ?? await _orderScheduleRepository.GetByIdAsync(jobInstance.OrderScheduleId);
                    originalStudentId = null;
                    currentAssignmentId = 0;
                }
            }
            else
            {

                var assignment = await _scheduleAssignmentRepository.LoadAssignmentWithIncludes(
                    scheduleAssignmentId!.Value,
                    new AssignmentIncludeOptions
                    {
                        IncludeSchedules = true,
                        IncludeOrder = true
                    });

                if (assignment == null)
                {
                    throw new ArgumentException($"Assignment {scheduleAssignmentId} not found");
                }

                orderSchedule = assignment.OrderSchedule;
                order = assignment.OrderSchedule.Order;
                originalStudentId = assignment.StudentId;
                currentAssignmentId = assignment.Id;
            }

            // Check if there's already an active reassignment for this entity
            bool hasActiveReassignment;
            if (jobInstanceId.HasValue)
            {
                hasActiveReassignment = await _reassignmentRecordRepository.HasActiveReassignmentsForInstanceAsync(jobInstanceId.Value);
            }
            else
            {

                hasActiveReassignment = await _reassignmentRecordRepository.HasActiveReassignmentsForAssignmentAsync(scheduleAssignmentId!.Value);


            }


            if (hasActiveReassignment)
            {
                _logger.LogWarning("⚠️ Active reassignment already exists for {EntityId}", jobInstanceId ?? scheduleAssignmentId);
                throw new InvalidOperationException("Active reassignment already exists for this entity");
            }

            // Create the reassignment record
            var reassignmentRecord = new ReassignmentRecord
            {
                ReassignJobInstanceId = jobInstanceId,
                ReassignAssignmentId = scheduleAssignmentId,
                CurrentAssignmentId = currentAssignmentId,
                OrderScheduleId = orderSchedule.Id,
                OrderId = order.Id,
                ReassignmentType = reassignmentType,
                Trigger = trigger,
                Reason = reason,
                RequestedByUserId = requestedByUserId,
                OriginalStudentId = originalStudentId,
                Status = ReassignmentStatus.Requested,
                PreferredStudentId = preferedStudentId,
                RequestedAt = DateTime.UtcNow
            };

            await _reassignmentRecordRepository.AddAsync(reassignmentRecord);

            // Make order schedule allow scheduling
            // for CompleteTakeover we track in OrderSchedule
            // for OneDaySub we track in ReassignmentRecord

            // if (reassignmentType == ReassignmentType.CompleteTakeover)
            // {
            //     orderSchedule.AllowAutoScheduling = true;
            //     orderSchedule.AutoScheduleAttemptCount = 0;
            //     orderSchedule.AutoScheduleDisableReason = null;
            //     await _orderScheduleRepository.UpdateAsync(orderSchedule);
            // }



            // Handle the reassignment based on type
            if (reassignmentType == ReassignmentType.CompleteTakeover)
            {
                await HandleCompleteTakeover(reassignmentRecord);
            }
            else if (reassignmentType == ReassignmentType.OneDaySubstitution)
            {
                await HandleOneDaySubstitution(reassignmentRecord);
            }

            // Auto-complete when admin provides a preferred student
            if (preferedStudentId.HasValue)
            {
                _reassignmentRecordRepository.Detach(reassignmentRecord);
                await CompleteReassignment(reassignmentRecord.Id, preferedStudentId.Value);
            }
            else
            {
                await NotifyAdminAboutReassignment(reassignmentRecord, order.SeniorId, NotificationType.ReassignmentStarted);
            }

            return reassignmentRecord;
        }

        public async Task CompleteReassignment(int reassignmentRecordId, int newStudentId)
        {
            _logger.LogInformation("✅ Completing reassignment: {RecordId}, NewStudent={StudentId}",
                reassignmentRecordId, newStudentId);

            var reassignmentRecord = await _reassignmentRecordRepository.GetByIdAsync(reassignmentRecordId, new ReassignmentIncludeOptions { });

            if (reassignmentRecord == null)
                throw new ArgumentException($"Reassignment record {reassignmentRecordId} not found");


            // Update the reassignment record
            reassignmentRecord.Status = ReassignmentStatus.Assigned;
            reassignmentRecord.AllowAutoScheduling = false;
            reassignmentRecord.NewStudentId = newStudentId;
            reassignmentRecord.CompletedAt = DateTime.UtcNow;
            await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

            _reassignmentRecordRepository.Detach(reassignmentRecord);

            // Handle post-completion actions based on reassignment type
            if (reassignmentRecord.ReassignmentType == ReassignmentType.CompleteTakeover)
            {
                await HandleCompleteTakeoverCompletion(reassignmentRecord);
            }
            else if (reassignmentRecord.ReassignmentType == ReassignmentType.OneDaySubstitution)
            {
                await HandleOneDaySubstitutionCompletion(reassignmentRecord);
            }

            // Notify relevant parties
            await NotifyAdminAboutReassignment(reassignmentRecord, reassignmentRecord.Order!.SeniorId, NotificationType.ReassignmentCompleted);
        }


        public async Task RecordReassignmentAttempt(int reassignmentRecordId, bool success = false)
        {
            var reassignmentRecord = await _reassignmentRecordRepository.GetByIdAsync(reassignmentRecordId, new ReassignmentIncludeOptions { });
            if (reassignmentRecord == null)
                throw new ArgumentException($"Reassignment record {reassignmentRecordId} not found");

            reassignmentRecord.AttemptCount++;
            reassignmentRecord.LastAttemptAt = DateTime.UtcNow;

            if (reassignmentRecord.AttemptCount >= reassignmentRecord.MaxAttempts && !success)
            {
                reassignmentRecord.Status = ReassignmentStatus.MaxAttemptsReached;
                reassignmentRecord.AllowAutoScheduling = false;
                await NotifyReassignmentFailure(reassignmentRecord);
            }

            await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);
        }


        public async Task ReassignExpiredContractJobs(int studentId)
        {
            _logger.LogInformation("🔁 Reassigning jobs for student with expired contract: {StudentId}", studentId);

            // Get all active assignments for the student
            var activeAssignments = await _scheduleAssignmentRepository.GetActiveAssignmentsByStudentId(studentId);

            foreach (var assignment in activeAssignments)
            {
                var systemAdminId = await GetAdminId();
                await InitiateReassignment(
                    ReassignmentType.CompleteTakeover,
                    ReassignmentTrigger.ContractExpiration,
                    "Student contract expired",
                    systemAdminId,
                    null,
                    assignment.Id);
            }

            _logger.LogInformation("✅ Completed reassigning all jobs for student: {StudentId}", studentId);
        }

        public async Task ReassignJobInstance(int jobInstanceId, ReassignmentType reassignmentType, string reason)
        {
            _logger.LogInformation("🔁 Reassigning job instance: {JobInstanceId}, Type: {Type}, Reason: {Reason}",
                jobInstanceId, reassignmentType, reason);

            await InitiateReassignment(
                reassignmentType,
                ReassignmentTrigger.AdminIntervention,
                reason,
                0, // System user
                jobInstanceId,
                null);
        }

        public async Task ReassignAssignment(int assignmentId, ReassignmentType reassignmentType, string reason)
        {
            _logger.LogInformation("🔁 Reassigning assignment: {AssignmentId}, Type: {Type}, Reason: {Reason}",
                assignmentId, reassignmentType, reason);

            await InitiateReassignment(
                reassignmentType,
                ReassignmentTrigger.AdminIntervention,
                reason,
                0, // System user
                null,
                assignmentId);
        }

        #region Private Methods

        private async Task HandleCompleteTakeover(ReassignmentRecord reassignmentRecord)
        {
            if (!reassignmentRecord.ReassignAssignmentId.HasValue)
                throw new InvalidOperationException("ScheduleAssignmentId is required for complete takeover");

            var assignment = await _scheduleAssignmentRepository.GetByIdAsync(reassignmentRecord.ReassignAssignmentId.Value);
            if (assignment == null)
                throw new ArgumentException($"Assignment {reassignmentRecord.ReassignAssignmentId} not found");

            // Mark current assignment as terminated
            assignment.Status = AssignmentStatus.Terminated;
            assignment.TerminationReason = GetTerminationReason(reassignmentRecord.Trigger);
            assignment.TerminatedAt = DateTime.UtcNow;
            await _scheduleAssignmentRepository.UpdateAsync(assignment);

            // Update reassignment record status
            reassignmentRecord.Status = ReassignmentStatus.InProgress;
            reassignmentRecord.AllowAutoScheduling = true;
            reassignmentRecord.LastAttemptAt = DateTime.UtcNow;
            await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);

            await MarkJobInstancesForCompleteTakeover(assignment.Id);

            // Revert order status to Pending since it no longer has a full assignment
            var order = await _orderRepository.GetByIdAsync(assignment.OrderId);
            if (order != null && order.Status == OrderStatus.FullAssigned)
            {
                order.Status = OrderStatus.Pending;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);
                _logger.LogInformation("📋 Order {OrderId} status changed from FullAssigned to Pending (student assignment terminated)", order.Id);
            }

            // v2: No auto-matching — admin manually assigns replacement
        }

        private async Task MarkJobInstancesForCompleteTakeover(int assignmentId)
        {
            // Update all job instances to show they not assigned
            var upComingJobInstances = await _jobInstanceRepository.GetJobInstancesAsync(
                assignmentId: assignmentId,
                prevAssignmentId: null,
                JobInstanceStatus.Upcoming,
                new SessionIncludeOptions { }
                );

            foreach (var instance in upComingJobInstances)
            {
                instance.ScheduleAssignmentId = null;
                instance.ScheduleAssignment = null;
                instance.PrevAssignmentId = assignmentId;
                instance.NeedsSubstitute = true;
                instance.ContractId = null;
            }

            await _jobInstanceRepository.UpdateRangeAsync(upComingJobInstances);
        }

        private async Task HandleOneDaySubstitution(ReassignmentRecord reassignmentRecord)
        {
            if (!reassignmentRecord.ReassignJobInstanceId.HasValue)
                throw new InvalidOperationException("JobInstanceId is required for one-day substitution");

            var jobInstance = await _jobInstanceRepository.GetByIdAsync(reassignmentRecord.ReassignJobInstanceId.Value);
            if (jobInstance == null)
                throw new ArgumentException($"Job instance {reassignmentRecord.ReassignJobInstanceId} not found");

            // Mark instance as needing substitution
            jobInstance.ScheduleAssignmentId = null;
            jobInstance.ScheduleAssignment = null;
            jobInstance.NeedsSubstitute = true;
            await _jobInstanceRepository.UpdateAsync(jobInstance);

            // Update reassignment record status
            reassignmentRecord.Status = ReassignmentStatus.InProgress;
            reassignmentRecord.AllowAutoScheduling = true;
            reassignmentRecord.LastAttemptAt = DateTime.UtcNow;
            await _reassignmentRecordRepository.UpdateAsync(reassignmentRecord);


            // v2: No auto-matching for substitutions — admin manually assigns replacement

        }

        private async Task HandleCompleteTakeoverCompletion(ReassignmentRecord reassignmentRecord)
        {
            if (reassignmentRecord.ReassignmentType != ReassignmentType.CompleteTakeover)
            {
                return;
            }

            if (!reassignmentRecord.NewStudentId.HasValue)
            {
                return;
            }

            // 
            var originalAssignment = await _scheduleAssignmentRepository.GetByIdAsync(reassignmentRecord.ReassignAssignmentId!.Value);

            if (originalAssignment == null)
                return;

            var newAssignment = new ScheduleAssignment
            {
                OrderScheduleId = originalAssignment.OrderScheduleId,
                OrderId = originalAssignment.OrderId,
                StudentId = reassignmentRecord.NewStudentId.Value,
                Status = AssignmentStatus.Accepted,
                PrevAssignmentId = originalAssignment.Id,
                AssignedAt = DateTime.UtcNow,
                AcceptedAt = DateTime.UtcNow
            };

            await _scheduleAssignmentRepository.AddAsync(newAssignment);

            // Update all job instances to point to the new assignment
            var upComingJobInstances = await _jobInstanceRepository.GetJobInstancesAsync(
                assignmentId: null,
                prevAssignmentId: originalAssignment.Id,
                JobInstanceStatus.Upcoming,
                new SessionIncludeOptions { }
                );

            foreach (var instance in upComingJobInstances)
            {
                if (instance.ScheduleAssignmentId != null)
                {
                    _logger.LogInformation("[SKIP] Job {instanceId} already assigned", instance.Id);
                    continue;
                }

                instance.ScheduleAssignmentId = newAssignment.Id;
                instance.PrevAssignmentId = reassignmentRecord.CurrentAssignmentId;
                instance.NeedsSubstitute = false;
                instance.ContractId = null;
            }

            await _jobInstanceRepository.UpdateRangeAsync(upComingJobInstances);
        }

        private async Task HandleOneDaySubstitutionCompletion(ReassignmentRecord reassignmentRecord)
        {
            if (!reassignmentRecord.ReassignJobInstanceId.HasValue || !reassignmentRecord.NewStudentId.HasValue)
                return;

            // Get the job instance
            var jobInstance = await _jobInstanceRepository.LoadJobInstanceWithIncludes(
                reassignmentRecord.ReassignJobInstanceId.Value,
                new SessionIncludeOptions
                {
                    Assignment = true
                }
                );

            if (jobInstance == null)
                return;

            // Create a temporary assignment for this single instance
            var temporaryAssignment = new ScheduleAssignment
            {
                OrderScheduleId = jobInstance.OrderScheduleId,
                OrderId = jobInstance.OrderId,
                StudentId = reassignmentRecord.NewStudentId.Value,
                Status = AssignmentStatus.Accepted,
                IsJobInstanceSub = true,
                PrevAssignmentId = reassignmentRecord.CurrentAssignmentId,
                AssignedAt = DateTime.UtcNow,
                AcceptedAt = DateTime.UtcNow
            };

            await _scheduleAssignmentRepository.AddAsync(temporaryAssignment);

            // Update the job instance to point to the new assignment
            jobInstance.ScheduleAssignmentId = temporaryAssignment.Id;
            jobInstance.PrevAssignmentId = reassignmentRecord.CurrentAssignmentId;
            jobInstance.Status = JobInstanceStatus.Upcoming;
            jobInstance.NeedsSubstitute = false;
            await _jobInstanceRepository.UpdateAsync(jobInstance);
        }

        private TerminationReason GetTerminationReason(ReassignmentTrigger trigger)
        {
            return trigger switch
            {
                ReassignmentTrigger.ContractExpiration => TerminationReason.StudentContractExpired,
                ReassignmentTrigger.StudentRequest => TerminationReason.StudentRequested,
                ReassignmentTrigger.AdminIntervention => TerminationReason.AdminIntervention,
                _ => TerminationReason.AdminIntervention
            };
        }

        private async Task NotifyAdminAboutReassignment(ReassignmentRecord reassignmentRecord, int seniorId, NotificationType type)
        {
            try
            {
                var adminId = await GetAdminId();
                var notification = _notificationFactory.ReassignmentStartNotification(
                                            recieverId: adminId,
                                            record: reassignmentRecord,
                                            seniorId: seniorId,
                                            type: type
                                            );

                if (notification == null) return;

                await _notificationService.StoreAndNotifyAsync(notification!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FAILED] Sending notification to admin bout  reassignment #{id}", reassignmentRecord.Id);
            }
        }

        private async Task NotifyStudentAboutReassignment(
            ReassignmentRecord reassignmentRecord,
            int seniorId)
        {
            try
            {
                var studentId = (int)reassignmentRecord.OriginalStudentId!;
                var student = await _studentRepository.GetByIdAsync(studentId);
                var culture = student.Contact.LanguageCode ?? "en";

                HNotification notifcation;

                if (reassignmentRecord.ReassignmentType == ReassignmentType.OneDaySubstitution)
                {
                    notifcation = _notificationFactory.JobCancelledNotification(studentId,
                     reassignmentRecord.ReassignJobInstance!,
                     culture: culture
                     );
                }
                else
                {
                    notifcation = _notificationFactory.ScheduleAssignmentCancelledNotification(
                                            recieverId: studentId,
                                            scheduleAssignment: reassignmentRecord.ReassignAssignment!,
                                            seniorId: seniorId,
                                            culture: culture
                                            );
                }

                await _notificationService.SendNotificationAsync(studentId, notifcation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FAILED] Sending notification to student bout  reassignment #{id}", reassignmentRecord.Id);
            }
        }



        private async Task NotifyReassignmentFailure(ReassignmentRecord reassignmentRecord)
        {

            try
            {


                var adminId = await GetAdminId();
                var seniorId = (int)reassignmentRecord?.Order?.SeniorId!;

                if (reassignmentRecord.Status == ReassignmentStatus.MaxAttemptsReached)
                {
                    return;
                }
                // var type = NotificationType.ReassignmentFailed;

                // var notification = _notificationFactory.AdminJobReassignmentNotification(adminId,
                //                 reassignmentRecord,
                //                 seniorId,
                //                 type);

                // await _notificationService.StoreAndNotifyAsync(notification!);
            }
            catch (Exception)
            {
            }
        }


        private async Task<int> GetAdminId()
        {
            var users = await _userRepository.GetAllAsync();
            var admin = users.FirstOrDefault(u => u.UserType == UserType.Admin);
            return admin?.Id ?? 1;
        }


        #endregion
    }
}