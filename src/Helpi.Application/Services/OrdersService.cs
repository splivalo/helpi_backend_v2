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
            ScheduleCancellationHandler scheduleCancellationHandler
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

                        if (orderUpdateDto.Status == OrderStatus.Cancelled)
                        {
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
                        return _mapper.Map<OrderDto>(updatedOrder);
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error updating order {OrderId}", orderId);
                        throw new DomainException("Order update failed", ex);
                }
        }

        public async Task<bool> CancelOrderAsync(int orderId, OrderCancelDto cancelDto)
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

                        // Use the centralized handler to cancel the order (will cascade to schedules)
                        await _orderCancellationHandler.CancelOrderAsync(order);

                        await _unitOfWork.SaveChangesAsync();

                        // Run maintenance 
                        await _statusMaintenanceService.MaintainOrderStatuses(orderId);

                        _logger.LogInformation("Successfully cancelled order {OrderId}", orderId);
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
                        AssignmentsJobInstances = true,
                },
                asNoTracking: false);

                return order;
        }
}
