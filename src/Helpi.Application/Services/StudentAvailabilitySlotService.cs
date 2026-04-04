
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Events;
using Helpi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class StudentAvailabilitySlotService
{
        private readonly IStudentAvailabilitySlotRepository _repository;
        private readonly IMapper _mapper;
        private readonly IEventMediator _mediator;
        private readonly IScheduleAssignmentRepository _assignmentRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notificationService;
        private readonly INotificationFactory _notificationFactory;
        private readonly IOrderRepository _orderRepo;
        private readonly ILogger<StudentAvailabilitySlotService> _logger;
        private readonly IPricingConfigurationRepository _pricingConfigRepo;

        public StudentAvailabilitySlotService(
                IStudentAvailabilitySlotRepository repository,
                IEventMediator mediator,
                IMapper mapper,
                ILogger<StudentAvailabilitySlotService> logger,
                IScheduleAssignmentRepository assignmentRepo,
                IStudentRepository studentRepo,
                IUserRepository userRepo,
                INotificationService notificationService,
                INotificationFactory notificationFactory,
                IOrderRepository orderRepo,
                IPricingConfigurationRepository pricingConfigRepo
        )
        {
                _repository = repository;
                _assignmentRepo = assignmentRepo;
                _studentRepo = studentRepo;
                _userRepo = userRepo;
                _notificationService = notificationService;
                _notificationFactory = notificationFactory;
                _orderRepo = orderRepo;
                _pricingConfigRepo = pricingConfigRepo;
                _mapper = mapper;
                _mediator = mediator;
                _logger = logger;
        }

        public async Task<StudentAvailabilitySlotDto> GetByIdAsync(int studentId, byte dayOfWeek)
        {
                var slot = await _repository.GetByIdAsync(studentId, dayOfWeek);
                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }
        public async Task<List<StudentAvailabilitySlotDto>> GetSlotsByStudentAsync(int studentId)
        {
                var slots = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<StudentAvailabilitySlotDto>>(slots);
        }

        public async Task<IEnumerable<StudentAvailabilitySlotDto>> GetSlotsByDayAndTimeRangeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end)
        {
                var slots = await _repository.GetByDayAndTimeRangeAsync(studentId, day, start, end);
                return _mapper.Map<IEnumerable<StudentAvailabilitySlotDto>>(slots);
        }

        public async Task<StudentAvailabilitySlotDto> CreateSlotAsync(StudentAvailabilitySlotCreateDto dto)
        {
                var slot = _mapper.Map<StudentAvailabilitySlot>(dto);
                await _repository.AddAsync(slot);

                ReinitiateAllFailedMatches();

                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }

        public async Task<List<StudentAvailabilitySlotDto>> CreateSlotsAsync(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                var slots = dtos.Select(_mapper.Map<StudentAvailabilitySlot>).ToList();
                await _repository.AddRangeAsync(slots);

                ReinitiateAllFailedMatches();

                return slots.Select(_mapper.Map<StudentAvailabilitySlotDto>).ToList();
        }

        public async Task<StudentAvailabilitySlotDto> UpdateSlotAsync(int studentId, byte dayOfWeek, StudentAvailabilitySlotUpdateDto dto)
        {
                var slot = await _repository.GetByIdAsync(studentId, dayOfWeek);
                if (slot == null)
                {
                        throw new NotFoundException($"Slot for StudentId {studentId} on DayOfWeek {dayOfWeek} not found.");
                }
                slot.StartTime = dto.StartTime;
                slot.EndTime = dto.EndTime;

                await _repository.UpdateAsync(slot);

                ReinitiateAllFailedMatches();

                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }

        public async Task DeleteSlotAsync(int studentId, byte dayOfWeek)
        {
                var slot = await _repository.GetByIdAsync(studentId, dayOfWeek);
                if (slot == null)
                {
                        throw new NotFoundException($"Slot for StudentId {studentId} on DayOfWeek {dayOfWeek} not found.");
                }

                await _repository.DeleteAsync(slot);

                // Check for conflicts on the removed day
                await HandleAvailabilityConflicts(studentId, new List<byte> { dayOfWeek });
        }

        public async Task<List<StudentAvailabilitySlotDto>> UpdateSlotsAsync(List<StudentAvailabilitySlotCreateDto> dtos, bool isAdmin = false)
        {
                if (dtos == null || dtos.Count == 0)
                        throw new ArgumentException("No slots provided.");

                var studentId = dtos.First().StudentId;

                // Get current slots BEFORE update to detect removed days
                var currentSlots = await _repository.GetByStudentAsync(studentId);
                var currentDays = currentSlots.Select(s => s.DayOfWeek).ToHashSet();
                var newDays = dtos.Select(d => d.DayOfWeek).ToHashSet();
                var removedDays = currentDays.Except(newDays).ToList();

                // Upsert: remove all → create new
                await _repository.RemoveAllByStudentIdAsync(studentId);
                var slots = dtos.Select(_mapper.Map<StudentAvailabilitySlot>).ToList();
                await _repository.AddRangeAsync(slots);

                ReinitiateAllFailedMatches();

                // Process conflicts for removed days
                if (removedDays.Count > 0)
                {
                        await HandleAvailabilityConflicts(studentId, removedDays, isAdmin);
                }

                return slots.Select(_mapper.Map<StudentAvailabilitySlotDto>).ToList();
        }

        public async Task<List<StudentAvailabilitySlotDto>> DeleteSlotsAsync(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                if (dtos == null || dtos.Count == 0)
                        throw new ArgumentException("No slots provided.");

                var studentId = dtos.First().StudentId;
                var removedDays = dtos.Select(d => d.DayOfWeek).Distinct().ToList();

                var slots = dtos.Select(_mapper.Map<StudentAvailabilitySlot>).ToList();
                await _repository.DeleteRangeAsync(slots);

                // Process conflicts for removed days
                await HandleAvailabilityConflicts(studentId, removedDays);

                return slots.Select(_mapper.Map<StudentAvailabilitySlotDto>).ToList();
        }

        /// <summary>
        /// v2: When student removes/reduces availability, handle affected orders:
        /// 1. Terminate conflicting assignments (AvailabilityConflict)
        /// 2. Cancel all future sessions for those assignments
        /// 3. Set affected orders back to Pending (Processing)
        /// 4. Notify admin + senior
        /// </summary>
        private async Task HandleAvailabilityConflicts(int studentId, List<byte> removedDays, bool isAdmin = false)
        {
                var conflicting = await _assignmentRepo.GetConflictingAssignmentsAsync(studentId, removedDays);
                if (conflicting.Count == 0) return;

                // v2: Block availability change if any affected session starts within cutoff hours
                // Admin bypass — admins can change availability immediately
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (!isAdmin)
                {
                        var config = await _pricingConfigRepo.GetByIdAsync(1);
                        var cutoffHours = config?.StudentCancelCutoffHours ?? 6;
                        var cutoff = DateTime.UtcNow.AddHours(cutoffHours);

                        foreach (var assignment in conflicting)
                        {
                                var imminentSession = assignment.JobInstances
                                        .Where(ji => ji.Status == JobInstanceStatus.Upcoming)
                                        .Any(ji => ji.ScheduledDate.ToDateTime(ji.StartTime) <= cutoff
                                                && ji.ScheduledDate.ToDateTime(ji.StartTime) > DateTime.UtcNow);

                                if (imminentSession)
                                {
                                        throw new DomainException(
                                                $"Cannot change availability — an affected session starts within {cutoffHours} hour(s)");
                                }
                        }
                }

                var student = await _studentRepo.GetByIdAsync(studentId);
                var studentName = student.Contact.FullName;
                var adminIds = await _userRepo.GetAdminIdsAsync();

                _logger.LogInformation(
                        "⚠️ Student {StudentName} (ID:{StudentId}) availability change affects {Count} assignments",
                        studentName, studentId, conflicting.Count);

                var affectedOrderIds = new HashSet<int>();

                foreach (var assignment in conflicting)
                {
                        var schedule = assignment.OrderSchedule;
                        var order = schedule.Order;

                        // 1. Terminate assignment
                        assignment.Status = AssignmentStatus.Terminated;
                        assignment.TerminationReason = TerminationReason.AvailabilityConflict;
                        assignment.TerminatedAt = DateTime.UtcNow;
                        await _assignmentRepo.UpdateAsync(assignment);

                        // 2. Cancel all future sessions for this assignment
                        var futureSessions = assignment.JobInstances
                                .Where(ji => ji.ScheduledDate > today && ji.Status == JobInstanceStatus.Upcoming)
                                .ToList();

                        foreach (var session in futureSessions)
                        {
                                session.Status = JobInstanceStatus.Cancelled;
                        }

                        // 3. Notify student about cancelled sessions
                        var studentCulture = student.Contact.LanguageCode ?? "hr";
                        foreach (var session in futureSessions)
                        {
                                var studentNotification = _notificationFactory.JobCancelledNotification(
                                        studentId, session, studentCulture);
                                await _notificationService.StoreAndNotifyAsync(studentNotification);
                        }

                        affectedOrderIds.Add(order.Id);

                        // 4. Admin notification per affected schedule
                        var dayName = System.Globalization.CultureInfo.GetCultureInfo("hr-HR")
                                .DateTimeFormat.GetDayName((DayOfWeek)(schedule.DayOfWeek % 7));
                        var scheduleDesc = $"{dayName} {schedule.StartTime:HH:mm}-{schedule.EndTime:HH:mm}";

                        await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                                adminId => _notificationFactory.AvailabilityChangedNotification(
                                        adminId, studentName, order.Id, scheduleDesc));

                        _logger.LogInformation(
                                "✅ Assignment #{AssignmentId} terminated, {SessionCount} future sessions cancelled for Order #{OrderId}",
                                assignment.Id, futureSessions.Count, order.Id);
                }

                // 4. Set affected orders back to Pending and notify seniors
                foreach (var orderId in affectedOrderIds)
                {
                        var order = conflicting.First(a => a.OrderSchedule.Order.Id == orderId).OrderSchedule.Order;

                        if (order.Status == OrderStatus.FullAssigned)
                        {
                                order.Status = OrderStatus.Pending;
                                order.UpdatedAt = DateTime.UtcNow;
                                await _orderRepo.UpdateAsync(order);
                        }

                        // Senior push notification
                        var senior = order.Senior;
                        var culture = senior.Contact.LanguageCode ?? "hr";
                        var seniorNotification = _notificationFactory.OrderBackToProcessingNotification(
                                senior.CustomerId, orderId, culture);
                        await _notificationService.SendNotificationAsync(senior.CustomerId, seniorNotification);
                }
        }

        private void ReinitiateAllFailedMatches()
        {
                _ = Task.Run(async () =>
                {
                        try
                        {
                                await _mediator.Publish(new ReinitiateAllFailedMatchesEvent());
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "❌ Failed to reinitiate failed matches");
                        }
                });
        }

}