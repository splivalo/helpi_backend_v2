
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class ScheduleAssignmentService
{
        private readonly IScheduleAssignmentRepository _repository;
        private readonly IOrderScheduleRepository _orderScheduleRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly IReassignmentService _reassignmentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ScheduleAssignmentService> _logger;
        private readonly OrderStatusMaintenanceService _statusMaintenanceService;
        private readonly IHangfireRecurringJobService _recurringJobService;
        private readonly IPricingConfigurationRepository _pricingConfigRepo;
        private readonly IJobInstanceRepository _jobInstanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISignalRNotificationService _signalR;
        private readonly INotificationService _notificationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IStudentContractRepository _contractRepository;

        public ScheduleAssignmentService(
                IScheduleAssignmentRepository repository,
                IOrderScheduleRepository orderScheduleRepository,
                IStudentRepository studentRepository,
                IMapper mapper,
                IReassignmentService reassignmentService,
                IUnitOfWork unitOfWork,
                ILogger<ScheduleAssignmentService> logger,
                OrderStatusMaintenanceService statusMaintenanceService,
                IHangfireRecurringJobService recurringJobService,
                IPricingConfigurationRepository pricingConfigRepo,
                IJobInstanceRepository jobInstanceRepository,
                IUserRepository userRepository,
                ISignalRNotificationService signalR,
                INotificationService notificationService,
                IOrderRepository orderRepository,
                IStudentContractRepository contractRepository)
        {
                _repository = repository;
                _orderScheduleRepository = orderScheduleRepository;
                _studentRepository = studentRepository;
                _mapper = mapper;
                _reassignmentService = reassignmentService;
                _unitOfWork = unitOfWork;
                _logger = logger;
                _statusMaintenanceService = statusMaintenanceService;
                _recurringJobService = recurringJobService;
                _pricingConfigRepo = pricingConfigRepo;
                _jobInstanceRepository = jobInstanceRepository;
                _userRepository = userRepository;
                _signalR = signalR;
                _notificationService = notificationService;
                _orderRepository = orderRepository;
                _contractRepository = contractRepository;
        }

        /// <summary>
        /// Admin directly assigns a student to an order schedule.
        /// Validates travel buffer conflicts and terminates any existing active assignment.
        /// </summary>
        public async Task<ScheduleAssignmentDto> AdminDirectAssignAsync(ScheduleAssignmentCreateDto dto, bool notifyStudent = true)
        {
                _logger.LogInformation("Admin direct assign: Student {StudentId} → Schedule {ScheduleId}",
                        dto.StudentId, dto.OrderScheduleId);

                // Verify student is active (Active or ContractAboutToExpire)
                var student = await _studentRepository.GetByIdAsync(dto.StudentId)
                        ?? throw new DomainException($"Student {dto.StudentId} not found.");

                if (student.Status != StudentStatus.Active && student.Status != StudentStatus.ContractAboutToExpire)
                {
                        throw new DomainException(
                                $"Student {dto.StudentId} is not eligible for assignment. " +
                                $"Current status: {student.Status}. Only Active or ContractAboutToExpire students can be assigned.");
                }

                // Load OrderSchedule to get DayOfWeek, StartTime, EndTime
                var orderSchedule = await _orderScheduleRepository.GetByIdAsync(dto.OrderScheduleId)
                        ?? throw new DomainException($"OrderSchedule {dto.OrderScheduleId} not found.");

                // Check for time conflicts with the current configured travel buffer.
                var travelBufferMinutes = await GetTravelBufferMinutesAsync();
                var bufferedStart = orderSchedule.StartTime.AddMinutes(-travelBufferMinutes);
                var bufferedEnd = orderSchedule.EndTime.AddMinutes(travelBufferMinutes);

                var activeAssignments = await _repository.GetActiveAssignmentsByStudentId(dto.StudentId);
                foreach (var sa in activeAssignments)
                {
                        var saSchedule = await _orderScheduleRepository.GetByIdAsync(sa.OrderScheduleId);
                        if (saSchedule == null || saSchedule.Id == dto.OrderScheduleId) continue;

                        // Same day-of-week and overlapping time (with buffer)
                        if (saSchedule.DayOfWeek == orderSchedule.DayOfWeek
                                && saSchedule.StartTime < bufferedEnd
                                && saSchedule.EndTime > bufferedStart)
                        {
                                _logger.LogWarning(
                                        "Conflict detected: Student {StudentId} has assignment on schedule {ConflictScheduleId} " +
                                        "({Day} {Start}-{End}) overlapping with requested {ReqStart}-{ReqEnd} (incl. {Buffer}min buffer)",
                                        dto.StudentId, saSchedule.Id, saSchedule.DayOfWeek,
                                        saSchedule.StartTime, saSchedule.EndTime,
                                        orderSchedule.StartTime, orderSchedule.EndTime, travelBufferMinutes);

                                throw new DomainException(
                                        $"Student {dto.StudentId} has a schedule conflict on day {saSchedule.DayOfWeek} " +
                                        $"({saSchedule.StartTime}-{saSchedule.EndTime}). " +
                                        $"{travelBufferMinutes}-minute travel buffer required between sessions.");
                        }
                }

                // Terminate existing active/pending assignment if any
                var existing = await _repository.GetAssignmentForOrderScheduleAsync(dto.OrderScheduleId);
                if (existing != null && (existing.Status == AssignmentStatus.Accepted
                        || existing.Status == AssignmentStatus.PendingAcceptance))
                {
                        var previousStudentId = existing.StudentId;
                        existing.Status = AssignmentStatus.Terminated;
                        existing.TerminationReason = TerminationReason.AdminIntervention;
                        existing.TerminatedAt = DateTime.UtcNow;
                        await _repository.UpdateAsync(existing);

                        // Cancel existing JobInstances for the old assignment
                        var oldJobs = await _jobInstanceRepository.GetByAssignmentAsync(existing.Id);
                        foreach (var ji in oldJobs)
                        {
                                if (ji.Status == JobInstanceStatus.Upcoming)
                                {
                                        ji.Status = JobInstanceStatus.Cancelled;
                                        await _jobInstanceRepository.UpdateAsync(ji);
                                }
                        }

                        _logger.LogInformation("Terminated previous assignment {AssignmentId} for schedule {ScheduleId}",
                                existing.Id, dto.OrderScheduleId);
                }

                // Create new assignment as PendingAcceptance — student must accept
                var assignment = new ScheduleAssignment
                {
                        OrderScheduleId = dto.OrderScheduleId,
                        OrderId = orderSchedule.OrderId,
                        StudentId = dto.StudentId,
                        Status = AssignmentStatus.PendingAcceptance,
                        AssignedAt = DateTime.UtcNow,
                        PrevAssignmentId = existing?.Id
                };

                await _repository.AddAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created assignment {AssignmentId}: Student {StudentId} → Schedule {ScheduleId} (PendingAcceptance)",
                        assignment.Id, dto.StudentId, dto.OrderScheduleId);

                // Notify AFTER SaveChanges so the old student's pending query returns empty
                if (existing != null && existing.TerminationReason == TerminationReason.AdminIntervention)
                {
                        var previousStudentId = existing.StudentId;
                        if (previousStudentId != dto.StudentId)
                        {
                                var prevStudent = await _studentRepository.GetByIdAsync(previousStudentId);
                                if (prevStudent != null)
                                {
                                        await _signalR.SendNotificationToUserAsync(prevStudent.UserId,
                                                new DTOs.HNotificationDto
                                                {
                                                        Title = "Dodjela poništena",
                                                        Body = "Admin vam je uklonio dodijeljenu narudžbu.",
                                                        Type = NotificationType.AssignmentRevoked,
                                                        CreatedAt = DateTime.UtcNow
                                                });
                                }
                        }
                }

                // Notify student about new pending assignment
                if (notifyStudent)
                {
                        var assignedStudent = await _studentRepository.GetByIdAsync(dto.StudentId);
                        if (assignedStudent != null)
                        {
                                await _signalR.SendNotificationToUserAsync(assignedStudent.UserId,
                                        new DTOs.HNotificationDto
                                        {
                                                Title = "Nova narudžba",
                                                Body = "Dodijeljena vam je nova narudžba. Otvorite aplikaciju za prihvaćanje.",
                                                Type = NotificationType.AssignmentPending,
                                                CreatedAt = DateTime.UtcNow
                                        });
                        }
                }

                // Generate JobInstances immediately so senior sees the schedule
                await GenerateJobInstancesForAssignmentAsync(assignment, orderSchedule);

                // Update order status (Pending → FullAssigned if all schedules now have assignments)
                await _statusMaintenanceService.MaintainOrderStatuses(orderSchedule.OrderId);

                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        /// <summary>
        /// Admin bulk-assigns students to multiple schedules of the same order in one call.
        /// Creates all assignments first, then sends ONE AssignmentPending notification per
        /// unique student — prevents the race condition where the student loads the modal
        /// after the first schedule is assigned and sees an incomplete schedule list.
        /// </summary>
        public async Task AdminBulkAssignAsync(List<ScheduleAssignmentCreateDto> dtos)
        {
                if (dtos == null || dtos.Count == 0) return;

                // Create all assignments without notifying students yet
                foreach (var dto in dtos)
                {
                        await AdminDirectAssignAsync(dto, notifyStudent: false);
                }

                // Send one notification per unique student
                var studentIds = dtos.Select(d => d.StudentId).Distinct();
                foreach (var studentId in studentIds)
                {
                        var student = await _studentRepository.GetByIdAsync(studentId);
                        if (student != null)
                        {
                                await _signalR.SendNotificationToUserAsync(student.UserId,
                                        new DTOs.HNotificationDto
                                        {
                                                Title = "Nova narudžba",
                                                Body = "Dodijeljena vam je nova narudžba. Otvorite aplikaciju za prihvaćanje.",
                                                Type = NotificationType.AssignmentPending,
                                                CreatedAt = DateTime.UtcNow
                                        });
                        }
                }
        }

        /// <summary>
        /// Admin terminates the existing assignment on a schedule without creating a new one.
        /// Used when skipping a day in partial-availability assignment.
        /// </summary>
        public async Task AdminTerminateAssignmentAsync(int orderScheduleId)
        {
                _logger.LogInformation("Admin terminate assignment on schedule {ScheduleId}", orderScheduleId);

                var existing = await _repository.GetAssignmentForOrderScheduleAsync(orderScheduleId);
                if (existing == null || (existing.Status != AssignmentStatus.Accepted
                        && existing.Status != AssignmentStatus.PendingAcceptance))
                {
                        _logger.LogInformation("No active/pending assignment on schedule {ScheduleId} — nothing to terminate", orderScheduleId);
                        return;
                }

                var previousStudentId = existing.StudentId;
                existing.Status = AssignmentStatus.Terminated;
                existing.TerminationReason = TerminationReason.AdminIntervention;
                existing.TerminatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(existing);

                // Cancel any existing JobInstances for this assignment
                var jobInstances = await _jobInstanceRepository.GetByAssignmentAsync(existing.Id);
                foreach (var ji in jobInstances)
                {
                        if (ji.Status == JobInstanceStatus.Upcoming)
                        {
                                ji.Status = JobInstanceStatus.Cancelled;
                                await _jobInstanceRepository.UpdateAsync(ji);
                        }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Terminated assignment {AssignmentId} on schedule {ScheduleId}",
                        existing.Id, orderScheduleId);

                // Notify old student
                var student = await _studentRepository.GetByIdAsync(previousStudentId);
                if (student != null)
                {
                        await _signalR.SendNotificationToUserAsync(student.UserId,
                                new DTOs.HNotificationDto
                                {
                                        Title = "Dodjela poništena",
                                        Body = "Admin vam je uklonio dodijeljenu narudžbu.",
                                        Type = NotificationType.AssignmentRevoked,
                                        CreatedAt = DateTime.UtcNow
                                });
                }

                // Update order status
                var orderSchedule = await _orderScheduleRepository.GetByIdAsync(orderScheduleId);
                if (orderSchedule != null)
                {
                        await _statusMaintenanceService.MaintainOrderStatuses(orderSchedule.OrderId);
                }
        }

        private async Task<int> GetTravelBufferMinutesAsync()
        {
                var pricingConfig = await _pricingConfigRepo.GetByIdAsync(1);
                return pricingConfig?.TravelBufferMinutes ?? 15;
        }

        /// <summary>
        /// Generates the initial batch of JobInstances for a newly created assignment.
        /// Same logic as Hangfire's GenerateInstancesForAssignment but triggered immediately.
        /// </summary>
        private async Task GenerateJobInstancesForAssignmentAsync(ScheduleAssignment assignment, OrderSchedule orderSchedule)
        {
                var pricingConfig = await _pricingConfigRepo.GetByIdAsync(1);
                if (pricingConfig == null)
                {
                        _logger.LogWarning("⚠️ No pricing configuration found — skipping job instance generation");
                        return;
                }

                // Look up active contract for the assigned student
                DateOnly? contractEnd = null;
                var contracts = await _contractRepository.GetByStudentIdAsync(assignment.StudentId);
                var activeContract = contracts
                        .Where(c => c.Status == Domain.Enums.ContractStatus.Active)
                        .OrderByDescending(c => c.ExpirationDate)
                        .FirstOrDefault();
                if (activeContract != null)
                        contractEnd = activeContract.ExpirationDate;

                // Build navigation properties needed by GenerateInstancesForAssignment
                assignment.OrderSchedule = orderSchedule;
                assignment.JobInstances = new List<JobInstance>();

                var jobInstances = _recurringJobService.GenerateInstancesForAssignment(
                        assignment, pricingConfig, contractEndDate: contractEnd);

                if (jobInstances.Any())
                {
                        await _jobInstanceRepository.AddRangeAsync(jobInstances);
                        _logger.LogInformation("✅ Generated {Count} job instances for assignment {AssignmentId}",
                                jobInstances.Count, assignment.Id);
                }
        }

        public async Task<List<ScheduleAssignmentDto>> GetAssignmentsByStudentAsync(int studentId)
        {
                var sa = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<ScheduleAssignmentDto>>(sa);
        }

        public async Task<ScheduleAssignmentDto> CreateAssignmentAsync(ScheduleAssignmentCreateDto dto)
        {
                var assignment = _mapper.Map<ScheduleAssignment>(dto);
                await _repository.AddAsync(assignment);
                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        public async Task<StudentDto> GetActiveStudentForOrderScheduleAsync(int orderScheduleId)
        {
                var student = await _repository.GetActiveStudentForOrderScheduleAsync(orderScheduleId);
                return _mapper.Map<StudentDto>(student);
        }
        public async Task<ScheduleAssignmentDto> GetAssignmentForOrderScheduleAsync(int orderScheduleId)
        {
                var assignment = await _repository.GetAssignmentForOrderScheduleAsync(orderScheduleId);
                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        public async Task<(bool, string)> ReassignScheduleAssignment(int scheduleAssignmentId, int? preferedStudentId)
        {
                try
                {
                        var adminId = 1;
                        var requestedByUserId = adminId;

                        // Initiate reassignment with preferred student (if specified)
                        await _reassignmentService.InitiateReassignment(
                                ReassignmentType.CompleteTakeover,
                                ReassignmentTrigger.AdminIntervention,
                                "requested by admin",
                                requestedByUserId,
                                null,
                                scheduleAssignmentId,
                                preferedStudentId
                            );

                        return (true, "success");
                }
                catch (Exception)
                {

                        return (false, "fail");
                }
        }

        /// <summary>
        /// Student accepts a pending assignment. Generates job instances and updates order status.
        /// Uses double-check pattern: re-reads status after SaveChanges to guard against
        /// concurrent accept requests (double-tap / race condition).
        /// </summary>
        public async Task<ScheduleAssignmentDto> AcceptAssignmentAsync(int assignmentId, int studentUserId)
        {
                var assignment = await _repository.GetByIdAsync(assignmentId)
                        ?? throw new DomainException($"Assignment {assignmentId} not found.");

                // Verify the student owns this assignment
                var student = await _studentRepository.GetByIdAsync(assignment.StudentId)
                        ?? throw new DomainException("Student not found.");
                if (student.UserId != studentUserId)
                        throw new DomainException("You are not authorized to accept this assignment.");

                if (assignment.Status != AssignmentStatus.PendingAcceptance)
                        throw new DomainException($"Assignment is not pending acceptance. Current status: {assignment.Status}");

                assignment.Status = AssignmentStatus.Accepted;
                assignment.AcceptedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(assignment);

                await _unitOfWork.SaveChangesAsync();

                // Double-check: re-read to confirm we won the race (guard against concurrent double-tap).
                // If another request accepted the same assignment concurrently, the status will differ.
                var confirmed = await _repository.GetByIdAsync(assignmentId);
                if (confirmed == null || confirmed.Status != AssignmentStatus.Accepted)
                {
                        throw new DomainException("Assignment was already processed by another request.");
                }

                // Load order schedule for job instance generation
                var orderSchedule = await _orderScheduleRepository.GetByIdAsync(assignment.OrderScheduleId)
                        ?? throw new DomainException($"OrderSchedule {assignment.OrderScheduleId} not found.");

                // Generate JobInstances if not already created (admin-assign creates them immediately).
                // SAFETY: per-session (IsJobInstanceSub) assignments must NEVER generate a recurring
                // series — they always cover exactly one existing JobInstance that was reactivated
                // in-place by the admin. Generating from scratch here would create a full duplicate
                // series and corrupt the schedule.
                var existingJobs = await _jobInstanceRepository.GetByAssignmentAsync(assignment.Id);
                if (!existingJobs.Any() && !assignment.IsJobInstanceSub)
                {
                        await GenerateJobInstancesForAssignmentAsync(assignment, orderSchedule);
                }
                else if (!existingJobs.Any() && assignment.IsJobInstanceSub)
                {
                        _logger.LogWarning(
                                "⚠️ Sub-assignment {AssignmentId} accepted but no JobInstance is attached — skipping series generation to avoid duplicates.",
                                assignment.Id);
                }

                // Update order status (Pending → FullAssigned if all schedules have accepted assignments)
                await _statusMaintenanceService.MaintainOrderStatuses(orderSchedule.OrderId);

                // Only notify admins once — when ALL schedules are covered (order → FullAssigned)
                var updatedOrder = await _orderRepository.GetByIdAsync(orderSchedule.OrderId);
                if (updatedOrder?.Status == OrderStatus.FullAssigned)
                {
                        var seniorName = orderSchedule.Order?.Senior?.Contact?.FullName ?? "—";
                        var orderNum = orderSchedule.Order?.OrderNumber ?? 0;
                        var adminIds = await _userRepository.GetAdminIdsAsync();
                        await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                                adminId => new HNotification
                                {
                                        RecieverUserId = adminId,
                                        Title = "Student prihvatio narudžbu",
                                        Body = $"{student.Contact?.FullName}, {seniorName}, Narudžba #{orderNum}",
                                        TranslationKey = "Notifications.AssignmentAccepted",
                                        Type = NotificationType.AssignmentAccepted,
                                        CreatedAt = DateTime.UtcNow,
                                        OrderId = orderSchedule.OrderId,
                                        StudentId = assignment.StudentId,
                                        SeniorId = orderSchedule.Order?.SeniorId,
                                });
                }

                // Notify senior that their order is now active (student accepted)
                if (updatedOrder?.Senior != null)
                {
                        var culture = updatedOrder.Senior.Contact?.LanguageCode ?? "hr";
                        var isHr = string.Equals(culture, "hr", StringComparison.OrdinalIgnoreCase);
                        await _notificationService.StoreAndNotifyAsync(new HNotification
                        {
                                RecieverUserId = updatedOrder.Senior.CustomerId,
                                Title = isHr ? "Narudžba aktivna" : "Order active",
                                Body = isHr
                                        ? $"Student {student.Contact?.FullName ?? "—"} je prihvatio vaš termin. Vaša narudžba je sada aktivna."
                                        : $"Student {student.Contact?.FullName ?? "—"} has accepted your session. Your order is now active.",
                                TranslationKey = "Notifications.AssignmentAccepted",
                                Type = NotificationType.AssignmentAccepted,
                                CreatedAt = DateTime.UtcNow,
                                SeniorId = updatedOrder.SeniorId,
                                OrderId = updatedOrder.Id,
                        }, viaSignalR: true, viaFcm: true);
                }

                // For single-session (IsJobInstanceSub) reactivated assignments, additionally
                // notify the senior that their specific session is now confirmed.
                if (assignment.IsJobInstanceSub && existingJobs.Any())
                {
                        var job = existingJobs.First();
                        var senior = job.Senior;
                        if (senior != null)
                        {
                                var seniorCulture = senior.Contact?.LanguageCode ?? "hr";
                                var isHr = string.Equals(seniorCulture, "hr", StringComparison.OrdinalIgnoreCase);
                                var sessionDate = job.ScheduledDate.ToString("dd.MM.yyyy");
                                await _notificationService.StoreAndNotifyAsync(new HNotification
                                {
                                        RecieverUserId = senior.CustomerId,
                                        Title = isHr ? "Termin potvrđen" : "Session confirmed",
                                        Body = isHr
                                            ? $"Student {student.Contact?.FullName ?? "—"} je prihvatio vaš termin {sessionDate}."
                                            : $"Student {student.Contact?.FullName ?? "—"} accepted your session on {sessionDate}.",
                                        TranslationKey = "Notifications.AssignmentAccepted",
                                        Type = NotificationType.AssignmentAccepted,
                                        CreatedAt = DateTime.UtcNow,
                                        SeniorId = job.SeniorId,
                                        OrderId = job.OrderId,
                                }, viaSignalR: true, viaFcm: true);
                        }
                }

                _logger.LogInformation("Student {StudentId} accepted assignment {AssignmentId}", assignment.StudentId, assignmentId);
                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        /// <summary>
        /// Student declines a pending assignment. Admin gets notified to reassign.
        /// </summary>
        public async Task DeclineAssignmentAsync(int assignmentId, int studentUserId)
        {
                var assignment = await _repository.GetByIdAsync(assignmentId)
                        ?? throw new DomainException($"Assignment {assignmentId} not found.");

                // Verify the student owns this assignment
                var student = await _studentRepository.GetByIdAsync(assignment.StudentId)
                        ?? throw new DomainException("Student not found.");
                if (student.UserId != studentUserId)
                        throw new DomainException("You are not authorized to decline this assignment.");

                if (assignment.Status != AssignmentStatus.PendingAcceptance)
                        throw new DomainException($"Assignment is not pending acceptance. Current status: {assignment.Status}");

                // Decline THIS assignment
                assignment.Status = AssignmentStatus.Declined;
                assignment.TerminatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(assignment);

                // Cancel JobInstances for the declined assignment
                var declinedJobs = await _jobInstanceRepository.GetByAssignmentAsync(assignment.Id);
                foreach (var ji in declinedJobs)
                {
                        if (ji.Status == JobInstanceStatus.Upcoming)
                        {
                                ji.Status = JobInstanceStatus.Cancelled;
                                await _jobInstanceRepository.UpdateAsync(ji);
                        }
                }

                // Auto-decline ALL other pending assignments for same student + same order
                var orderSchedule = await _orderScheduleRepository.GetByIdAsync(assignment.OrderScheduleId);
                var orderId = orderSchedule?.OrderId;
                if (orderId != null)
                {
                        // Get all OrderScheduleIds for this order
                        var orderSchedules = await _orderScheduleRepository.GetByOrderAsync(orderId.Value);
                        var osIds = orderSchedules.Select(os => os.Id).ToHashSet();

                        // Find other pending assignments for same student on same order's schedules
                        var allForStudent = await _repository.GetByStudentAsync(assignment.StudentId);
                        var otherPending = allForStudent
                                .Where(a => a.Id != assignmentId
                                        && a.Status == AssignmentStatus.PendingAcceptance
                                        && osIds.Contains(a.OrderScheduleId))
                                .ToList();
                        foreach (var other in otherPending)
                        {
                                other.Status = AssignmentStatus.Declined;
                                other.TerminatedAt = DateTime.UtcNow;
                                await _repository.UpdateAsync(other);

                                // Cancel JobInstances for sibling declined assignment
                                var siblingJobs = await _jobInstanceRepository.GetByAssignmentAsync(other.Id);
                                foreach (var sji in siblingJobs)
                                {
                                        if (sji.Status == JobInstanceStatus.Upcoming)
                                        {
                                                sji.Status = JobInstanceStatus.Cancelled;
                                                await _jobInstanceRepository.UpdateAsync(sji);
                                        }
                                }
                        }
                        _logger.LogInformation("Auto-declined {Count} sibling assignments for order {OrderId}", otherPending.Count, orderId);
                }

                await _unitOfWork.SaveChangesAsync();

                // Update order status back to Pending
                if (orderSchedule != null)
                        await _statusMaintenanceService.MaintainOrderStatuses(orderSchedule.OrderId);

                // Send ONE notification for the entire order decline
                var seniorNameD = orderSchedule?.Order?.Senior?.Contact?.FullName ?? "—";
                var orderNumD = orderSchedule?.Order?.OrderNumber ?? 0;
                var adminIds = await _userRepository.GetAdminIdsAsync();
                await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                        adminId => new HNotification
                        {
                                RecieverUserId = adminId,
                                Title = "Student odbio narudžbu",
                                Body = $"{student.Contact?.FullName}, {seniorNameD}, Narudžba #{orderNumD}",
                                TranslationKey = "Notifications.AssignmentDeclined",
                                Type = NotificationType.AssignmentDeclined,
                                CreatedAt = DateTime.UtcNow,
                                OrderId = orderSchedule?.OrderId,
                                StudentId = assignment.StudentId,
                                SeniorId = orderSchedule?.Order?.SeniorId,
                        });

                _logger.LogInformation("Student {StudentId} declined all assignments for order {OrderId} (triggered by assignment {AssignmentId})",
                        assignment.StudentId, orderId, assignmentId);
        }

        /// <summary>
        /// Gets all pending assignments for a student (used by mobile app overlay).
        /// </summary>
        public async Task<List<object>> GetPendingAssignmentsByStudentUserIdAsync(int studentUserId)
        {
                var student = await _studentRepository.GetByUserIdAsync(studentUserId);
                if (student == null) return new List<object>();

                var allAssignments = await _repository.GetByStudentWithDetailsAsync(student.UserId);
                var pending = allAssignments
                        .Where(a => a.Status == AssignmentStatus.PendingAcceptance)
                        .ToList();

                // Group by order — one entry per order with schedule items
                var grouped = pending
                        .GroupBy(a => a.OrderId)
                        .Select(g =>
                        {
                                var first = g.OrderBy(a => a.Id).First();
                                return (object)new
                                {
                                        Id = first.Id,
                                        OrderId = first.OrderId,
                                        SeniorName = first.OrderSchedule?.Order?.Senior?.Contact?.FullName ?? "—",
                                        Address = first.OrderSchedule?.Order?.Senior?.Contact?.FullAddress ?? "",
                                        StartDate = first.OrderSchedule?.Order?.StartDate.ToString("yyyy-MM-dd"),
                                        EndDate = first.OrderSchedule?.Order?.EndDate.ToString("yyyy-MM-dd"),
                                        first.AssignedAt,
                                        AssignmentIds = g.Select(a => a.Id).ToList(),
                                        ScheduleItems = g.Select(a => new
                                        {
                                                a.OrderScheduleId,
                                                DayOfWeek = (int)(a.OrderSchedule?.DayOfWeek ?? 0),
                                                StartTime = a.OrderSchedule?.StartTime.ToString("HH:mm"),
                                                EndTime = a.OrderSchedule?.EndTime.ToString("HH:mm"),
                                        }).ToList(),
                                };
                        })
                        .ToList();

                return grouped;
        }

        public async Task<List<object>> GetAllPendingAcceptanceForAdminAsync()
        {
                var assignments = await _repository.GetAllPendingAcceptanceAsync();
                return assignments.Select(a => new
                {
                        a.Id,
                        a.OrderId,
                        a.OrderScheduleId,
                        a.AssignedAt,
                        a.IsJobInstanceSub,
                        JobInstanceId = a.IsJobInstanceSub
                            ? a.JobInstances.FirstOrDefault()?.Id
                            : (int?)null,
                        MinutesPending = (int)(DateTime.UtcNow - a.AssignedAt).TotalMinutes,
                        StudentName = a.Student?.Contact?.FullName ?? "—",
                        StudentId = a.StudentId,
                        SeniorName = a.OrderSchedule?.Order?.Senior?.Contact?.FullName ?? "—",
                        SeniorId = a.OrderSchedule?.Order?.SeniorId ?? 0,
                }).Cast<object>().ToList();
        }

}