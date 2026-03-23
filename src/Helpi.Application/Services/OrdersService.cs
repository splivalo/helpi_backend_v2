using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.Order;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
using Helpi.Application.Services.Maintenance;
using Microsoft.Extensions.Logging;
using Helpi.Application.Common.Extensions;
using Helpi.Application.Common.Interfaces;

namespace Helpi.Application.Services;

public class OrdersService
{
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderScheduleRepository _scheduleRepository;
        private readonly IOrderServiceRepository _orderServiceRepository;
        private readonly IMapper _mapper;

        private readonly IMatchingService _matchingService;
        private readonly IJobRequestRepository _jobRequestRepository;
        private readonly IJobInstanceRepository _jobInstanceRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<OrdersService> _logger;


        private readonly OrderStatusMaintenanceService _statusMaintenanceService;
        private readonly OrderCancellationHandler _orderCancellationHandler;
        private readonly ScheduleCancellationHandler _scheduleCancellationHandler;
        private readonly INotificationFactory _notificationFactory;

        private readonly INotificationService _notificationService;
        private readonly ISeniorRepository _seniorRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public OrdersService(
            IOrderRepository orderRepository,
            IOrderScheduleRepository scheduleRepository,
            IOrderServiceRepository orderServiceRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMatchingService matchingService,
            IJobRequestRepository jobRequestRepository,
            IJobInstanceRepository jobInstanceRepository,
            ILogger<OrdersService> logger,
            OrderStatusMaintenanceService statusMaintenanceService,
            OrderCancellationHandler orderCancellationHandler,
            ScheduleCancellationHandler scheduleCancellationHandler,
            INotificationFactory notificationFactory,
            INotificationService notificationService,
            ISeniorRepository seniorRepository,
            ICustomerRepository customerRepository,
            IUserRepository userRepository
        )
        {
                _orderRepository = orderRepository;
                _scheduleRepository = scheduleRepository;
                _orderServiceRepository = orderServiceRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _matchingService = matchingService;
                _jobRequestRepository = jobRequestRepository;
                _jobInstanceRepository = jobInstanceRepository;
                _logger = logger;
                _statusMaintenanceService = statusMaintenanceService;
                _orderCancellationHandler = orderCancellationHandler;
                _scheduleCancellationHandler = scheduleCancellationHandler;
                _notificationFactory = notificationFactory;
                _notificationService = notificationService;
                _seniorRepository = seniorRepository;
                _customerRepository = customerRepository;
                _userRepository = userRepository;
        }


        public async Task<List<OrderDto>> GetOrdersAsync(OrderStatus? status = null)
        {
                var orders = await _orderRepository.GetAllAsync(status);
                return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<List<OrderDto>> GetOrdersBySeniorAsync(int seniorId)
        {
                var orders = await _orderRepository.GetBySeniorAsync(seniorId);
                return _mapper.Map<List<OrderDto>>(orders);
        }

        private DateOnly AdjustStartDateToNearestWeekday(
    DateOnly startDate,
    IEnumerable<OrderScheduleCreateDto> schedules)
        {
                var allowedDays = schedules
                    .Select(s => DayOfWeekExtensions.FromIsoWeekday(s.DayOfWeek))
                    .Distinct()
                    .ToHashSet();

                var cursor = startDate;

                for (int i = 0; i < 7; i++) // max one week lookahead
                {
                        if (allowedDays.Contains(cursor.DayOfWeek))
                        {
                                _logger.LogInformation("Adjusted: startDate: {old} to NewStart: {new} -- .Net Day of week {DOW}", startDate, cursor, i);

                                return cursor;
                        }


                        cursor = cursor.AddDays(1);
                }

                throw new DomainException(
                    "❌  No valid weekday found within adjustment window."
                );
        }


        public async Task<OrderDto> CreateOrderAsync(OrderCreateDto orderCreateDto)
        {
                // Suspension check: prevent suspended users from creating orders
                var senior = await _seniorRepository.GetByIdAsync(orderCreateDto.SeniorId);
                if (senior == null)
                        throw new DomainException("❌ Senior not found.");

                var customer = await _customerRepository.GetByIdAsync(senior.CustomerId);
                if (customer == null)
                        throw new DomainException("❌ Customer not found.");

                var user = await _userRepository.GetByIdAsync(customer.UserId);
                if (user.IsSuspended)
                        throw new DomainException("❌ Cannot create order — user account is suspended.");

                try
                {
                        orderCreateDto.StartDate = AdjustStartDateToNearestWeekday(
                            orderCreateDto.StartDate,
                            orderCreateDto.Schedules
                        );

                        if (orderCreateDto.EndDate < orderCreateDto.StartDate)
                                throw new DomainException("❌ EndDate cannot be before StartDate.");

                        // === 1. Create Order ===
                        var order = _mapper.Map<Order>(orderCreateDto);
                        order = await _orderRepository.AddNoSaveAsync(order);

                        // === 2. Add Services ===
                        var orderServices = orderCreateDto.Services.Select(orderServiceCreateDto =>
                        {
                                var orderService = _mapper.Map<OrderService>(orderServiceCreateDto);
                                orderService.OrderId = order.Id; // Link to the created order
                                return orderService;
                        }).ToList();

                        await _orderServiceRepository.AddRangeNoSaveAsync(orderServices);

                        // === 3. Add Schedules ===
                        var orderSchedules = orderCreateDto.Schedules.Select(orderScheduleCreateDto =>
                        {
                                var orderSchedule = _mapper.Map<OrderSchedule>(orderScheduleCreateDto);
                                orderSchedule.OrderId = order.Id; // Link to the created order
                                return orderSchedule;
                        }).ToList();

                        await _scheduleRepository.AddRangeNoSaveAsync(orderSchedules);

                        // === 4. Commit Transaction ===
                        await _unitOfWork.SaveChangesAsync();

                        // === 5. Re-fetch Order with related data ===
                        var savedOrder = await _orderRepository.GetByIdAsync(order.Id);

                        // === 6. Post-Creation:  ===
                        await _matchingService.StartMatching(order.Id);

                        // === 7. Notify admins about new order ===
                        await NotifyAdminsAboutNewOrder(savedOrder);

                        return _mapper.Map<OrderDto>(savedOrder);
                }
                catch (Exception ex)
                {
                        throw new DomainException("❌ Order creation failed", ex);
                }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
                var order = await _orderRepository.GetByIdAsync(id);
                return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateOrderAsync(int orderId, OrderUpdateDto orderUpdateDto)
        {
                try
                {
                        _logger.LogInformation("Starting update for order {OrderId}", orderId);

                        // Get the existing order with related data
                        var order = await GetLoadedOrderById(orderId);
                        if (order == null)
                        {
                                _logger.LogWarning("Order {OrderId} not found", orderId);
                                throw new DomainException($"Order with ID {orderId} not found");
                        }

                        // Early exit for terminal states
                        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
                        {
                                _logger.LogInformation("⚠️ Order ID {OrderId} is in terminal state [{Status}]. Skipping...",
                                    orderId, order.Status);
                                throw new DomainException($"Order {orderId} cannot be modified, has status {order.Status}");
                        }

                        List<int> toBeCancelledScheduleIds = [];
                        if (orderUpdateDto.Status == OrderStatus.Cancelled)
                        {
                                // id's of schedules that are currently not cancelld but will be
                                toBeCancelledScheduleIds = order.Schedules
                                                               .Where(s => !s.IsCancelled)
                                                               .Select(s => s.Id)
                                                               .ToList();

                                _logger.LogInformation("Cancelling order {OrderId} as part of update", orderId);
                                await _orderCancellationHandler.CancelOrderAsync(order);
                        }
                        else
                        {
                                // Update basic order properties
                                UpdateOrderProperties(order, orderUpdateDto);

                                // Handle services
                                await HandleServiceUpdates(order, orderUpdateDto);

                                // Handle schedules (note: uses new ScheduleCancellationHandler)
                                await HandleScheduleUpdates(order, orderUpdateDto);
                        }

                        await _unitOfWork.SaveChangesAsync();



                        _logger.LogInformation("Successfully updated order {OrderId}", orderId);

                        // Run maintenance 
                        await _statusMaintenanceService.MaintainOrderStatuses(orderId);

                        await _matchingService.StartMatching(order.Id);

                        // Refetch to get updated relationships
                        var updatedOrder = await GetLoadedOrderById(orderId);

                        if (updatedOrder?.Status == OrderStatus.Cancelled)
                        {

                                await NotifySeniorAboutOrderCancel(order);
                                await NotifyStudentsAboutOrderCancel(order, toBeCancelledScheduleIds);
                        }

                        return _mapper.Map<OrderDto>(updatedOrder);
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error updating order {OrderId}", orderId);
                        throw new DomainException("Order update failed", ex);
                }
        }

        public async Task<bool> CancelOrderAsync(int orderId, OrderCancelDto cancelDto, bool isAdmin = false)
        {
                try
                {
                        _logger.LogInformation("Cancelling order {OrderId}", orderId);

                        var order = await GetLoadedOrderById(orderId);


                        if (order == null)
                        {
                                _logger.LogWarning("Order {OrderId} not found for cancellation", orderId);
                                throw new DomainException($"Order with ID {orderId} not found");
                        }

                        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
                        {
                                _logger.LogInformation("⚠️ Order ID {OrderId} is in terminal state [{Status}]. Skipping...",
                                    orderId, order.Status);
                                throw new DomainException($"Order {orderId} cannot be modified, has status {order.Status}");
                        }

                        // v2: Non-admin users cannot cancel if any upcoming session starts within 24h
                        if (!isAdmin)
                        {
                                var now = DateTime.UtcNow;
                                var nearestUpcoming = order.Schedules
                                        .SelectMany(s => s.Assignments)
                                        .SelectMany(a => a.JobInstances)
                                        .Where(ji => ji.Status == JobInstanceStatus.Upcoming)
                                        .Select(ji => ji.ScheduledDate.ToDateTime(ji.StartTime))
                                        .Where(dt => dt > now)
                                        .OrderBy(dt => dt)
                                        .FirstOrDefault();

                                if (nearestUpcoming != default && nearestUpcoming <= now.AddHours(24))
                                {
                                        throw new DomainException("Cannot cancel order — the next session starts within 24 hours");
                                }
                        }

                        // id's of schedules that are currently not cancelld but will be
                        List<int> toBeCancelledScheduleIds = order.Schedules
                                                        .Where(s => !s.IsCancelled)
                                                        .Select(s => s.Id)
                                                        .ToList();

                        // Use the centralized handler to cancel the order (will cascade to schedules)
                        await _orderCancellationHandler.CancelOrderAsync(order);

                        await _unitOfWork.SaveChangesAsync();

                        // Run maintenance 
                        await _statusMaintenanceService.MaintainOrderStatuses(orderId);

                        _logger.LogInformation("Successfully cancelled order {OrderId}", orderId);

                        await NotifySeniorAboutOrderCancel(order);
                        await NotifyStudentsAboutOrderCancel(order, toBeCancelledScheduleIds);
                        await NotifyAdminsAboutOrderCancel(order);

                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                        throw new DomainException("Order cancellation failed", ex);
                }
        }



        private void UpdateOrderProperties(Order order, OrderUpdateDto updateDto)
        {
                if (updateDto.PaymentMethodId.HasValue) order.PaymentMethodId = updateDto.PaymentMethodId;
                if (updateDto.StartDate.HasValue) order.StartDate = updateDto.StartDate.Value;
                if (updateDto.EndDate.HasValue) order.EndDate = updateDto.EndDate.Value;
                if (updateDto.Status.HasValue) order.Status = updateDto.Status.Value;
                // Handle PromoCodeId: 0 = remove, positive = set/change, null = no change
                if (updateDto.PromoCodeId.HasValue)
                {
                        order.PromoCodeId = updateDto.PromoCodeId.Value == 0 ? null : updateDto.PromoCodeId.Value;
                }
        }

        private async Task HandleServiceUpdates(Order order, OrderUpdateDto updateDto)
        {
                // Remove services
                if (updateDto.ServiceIdsToRemove.Any())
                {
                        var servicesToRemove = order.OrderServices
                            .Where(s => updateDto.ServiceIdsToRemove.Contains(s.ServiceId))
                            .ToList();

                        _orderServiceRepository.MarkForDelete(servicesToRemove);
                        _logger.LogDebug("Marked services for delete");
                }

                // Add new services
                if (updateDto.ServicesToAdd.Any())
                {
                        var newServices = updateDto.ServicesToAdd.Select(dto =>
                        {
                                var service = _mapper.Map<OrderService>(dto);
                                service.OrderId = order.Id;
                                return service;
                        }).ToList();

                        await _orderServiceRepository.AddRangeNoSaveAsync(newServices);
                        _logger.LogDebug("Added {Count} new services to order {OrderId}", newServices.Count, order.Id);
                }
        }

        private async Task HandleScheduleUpdates(Order order, OrderUpdateDto updateDto)
        {
                // Remove schedules
                if (updateDto.ScheduleIdsToRemove.Any())
                {
                        var schedulesToRemove = order.Schedules
                            .Where(s => updateDto.ScheduleIdsToRemove.Contains(s.Id))
                            .ToList();

                        foreach (var schedule in schedulesToRemove)
                        {
                                // Delegate cancellation logic to ScheduleCancellationHandler
                                await _scheduleCancellationHandler.CancelScheduleAsync(schedule, "Removed during order update");
                                _logger.LogDebug("Removed schedule {ScheduleId} from order {OrderId}", schedule.Id, order.Id);
                        }
                }

                // Add new schedules
                if (updateDto.SchedulesToAdd.Any())
                {
                        var newSchedules = updateDto.SchedulesToAdd.Select(dto =>
                        {
                                var schedule = _mapper.Map<OrderSchedule>(dto);
                                schedule.OrderId = order.Id;
                                return schedule;
                        }).ToList();

                        await _scheduleRepository.AddRangeNoSaveAsync(newSchedules);
                        _logger.LogDebug("Added {Count} new schedules to order {OrderId}", newSchedules.Count, order.Id);
                }
        }

        private async Task<Order?> GetLoadedOrderById(int orderId)
        {
                var order = await _orderRepository.LoadOrderWithIncludes(orderId, new OrderIncludeOptions
                {
                        Senior = true,

                        OrderServices = true,
                        Schedules = true,
                        SchedulesJobRequests = true,
                        ScheduleAssignments = true,
                        ScheduleAssignmentStudent = true,
                        AssignmentsJobInstances = true,
                },
                asNoTracking: false);

                return order;
        }


        private async Task NotifySeniorAboutOrderCancel(Order order)
        {
                try
                {
                        var recieverId = order.Senior.CustomerId;

                        var notification = _notificationFactory.SeniorOrderCancelledNotification(
                                receiverUserId: recieverId,
                                        order: order,
                                        culture: order.Senior.Contact.LanguageCode ?? "hr");

                        await _notificationService.SendNotificationAsync(recieverId, notification);
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to send cancellation notification. OrderId={orderId}",
                                                          order.Id);
                }
        }



        private async Task NotifyStudentsAboutOrderCancel(
     Order order,
     List<int> cancelledScheduleIds)
        {
                var cancelledScheduleSet = cancelledScheduleIds.ToHashSet();

                var assignments = order.Schedules.SelectMany(s => s.Assignments);

                foreach (var assignment in assignments)
                {
                        if (!cancelledScheduleSet.Contains(assignment.OrderScheduleId))
                                continue;

                        if (assignment.Student == null)
                                continue;

                        try
                        {
                                var student = assignment.Student;

                                var notification =
                                    _notificationFactory.ScheduleAssignmentCancelledNotification(
                                        recieverId: assignment.StudentId,
                                        scheduleAssignment: assignment,
                                        seniorId: order.SeniorId,
                                        culture: student.Contact.LanguageCode ?? "hr");

                                await _notificationService.SendNotificationAsync(
                                    student.UserId,
                                    notification);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(
                                    ex,
                                    "❌  Failed to send cancellation notification. AssignmentId={AssignmentId}",
                                    assignment.Id);
                        }
                }
        }

        private async Task NotifyAdminsAboutOrderCancel(Order order)
        {
                try
                {
                        var adminIds = await _userRepository.GetAdminIdsAsync();
                        await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                                adminId => _notificationFactory.AdminOrderCancelledNotification(adminId, order));
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to send order cancel notification to admins. OrderId={OrderId}", order.Id);
                }
        }

        private async Task NotifyAdminsAboutNewOrder(Order order)
        {
                try
                {
                        var adminIds = await _userRepository.GetAdminIdsAsync();
                        await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                                adminId => _notificationFactory.AdminNewOrderNotification(adminId, order));
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to send new order notification to admins. OrderId={OrderId}", order.Id);
                }
        }

        #region Archive Methods

        /// <summary>
        /// Checks if order can be archived and returns blocking item counts.
        /// </summary>
        public async Task<ArchiveCheckDto> GetArchiveCheckAsync(int orderId)
        {
                var order = await _orderRepository.LoadOrderWithIncludes(orderId, new OrderIncludeOptions
                {
                        Schedules = true,
                        ScheduleAssignments = true,
                        AssignmentsJobInstances = true
                });

                if (order == null)
                {
                        return new ArchiveCheckDto
                        {
                                CanArchiveDirectly = false,
                                HasBlockingItems = false,
                                Message = "Narudžba nije pronađena"
                        };
                }

                // Order can be archived directly if it's already cancelled or completed
                var isAlreadyTerminal = order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed;

                if (isAlreadyTerminal)
                {
                        return new ArchiveCheckDto
                        {
                                CanArchiveDirectly = true,
                                HasBlockingItems = false,
                                Message = "Narudžba je već završena i može se arhivirati."
                        };
                }

                // Count active schedules and upcoming sessions
                var activeSchedules = order.Schedules.Where(s => !s.IsCancelled).ToList();
                var upcomingSessions = order.Schedules
                        .SelectMany(s => s.Assignments)
                        .SelectMany(a => a.JobInstances)
                        .Where(j => j.Status == JobInstanceStatus.Upcoming)
                        .ToList();

                return new ArchiveCheckDto
                {
                        CanArchiveDirectly = false,
                        HasBlockingItems = true,
                        ActiveAssignmentsCount = activeSchedules.Count,
                        UpcomingSessionsCount = upcomingSessions.Count,
                        Message = $"Narudžba ima {activeSchedules.Count} aktivnih termina i {upcomingSessions.Count} nadolazećih sesija. Sve će biti otkazano."
                };
        }

        /// <summary>
        /// Archives an order. If force=true, cancels all schedules and sessions first.
        /// </summary>
        public async Task<ArchiveResultDto> ArchiveOrderAsync(int orderId, ArchiveRequestDto request)
        {
                _logger.LogInformation("📦 Archiving order {OrderId}, Force={Force}", orderId, request.Force);

                var order = await _orderRepository.LoadOrderWithIncludes(orderId, new OrderIncludeOptions
                {
                        Schedules = true,
                        ScheduleAssignments = true,
                        ScheduleAssignmentStudent = true,
                        AssignmentsJobInstances = true,
                        SchedulesJobRequests = true
                }, asNoTracking: false);

                if (order == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Order not found" };
                }

                var check = await GetArchiveCheckAsync(orderId);

                // If already terminal, just archive
                if (check.CanArchiveDirectly)
                {
                        order.IsArchived = true;
                        await _orderRepository.UpdateAsync(order);

                        return new ArchiveResultDto
                        {
                                Success = true,
                                Message = "Narudžba uspješno arhivirana"
                        };
                }

                // If not terminal and not force, return error
                if (!request.Force)
                {
                        return new ArchiveResultDto
                        {
                                Success = false,
                                Message = check.Message
                        };
                }

                // Force: cancel the order first
                _logger.LogInformation("🔄 Force archiving - cancelling order {OrderId}", orderId);

                await _orderCancellationHandler.CancelOrderAsync(order);
                order.IsArchived = true;
                await _orderRepository.UpdateAsync(order);

                // Notify affected students
                var cancelledScheduleIds = order.Schedules.Where(s => s.IsCancelled).Select(s => s.Id).ToList();
                await NotifyStudentsAboutOrderCancel(order, cancelledScheduleIds);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("✅ Order {OrderId} archived successfully", orderId);

                return new ArchiveResultDto
                {
                        Success = true,
                        Message = "Narudžba uspješno arhivirana",
                        CancelledSessionsCount = check.UpcomingSessionsCount,
                        TerminatedAssignmentsCount = check.ActiveAssignmentsCount
                };
        }

        /// <summary>
        /// Unarchives an order by clearing the IsArchived flag.
        /// </summary>
        public async Task<ArchiveResultDto> UnarchiveOrderAsync(int orderId)
        {
                _logger.LogInformation("📦 Unarchiving order {OrderId}", orderId);

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Order not found" };
                }

                if (!order.IsArchived)
                {
                        return new ArchiveResultDto { Success = false, Message = "Order is not archived" };
                }

                order.IsArchived = false;
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation("✅ Order {OrderId} unarchived successfully", orderId);

                return new ArchiveResultDto
                {
                        Success = true,
                        Message = "Narudžba uspješno vraćena iz arhive"
                };
        }

        #endregion

}



