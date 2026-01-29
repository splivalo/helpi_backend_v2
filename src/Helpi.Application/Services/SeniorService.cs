
using System.Text.Json;
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services.Maintenance;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class SeniorService
{
        private readonly ISeniorRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SeniorService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly OrderCancellationHandler _orderCancellationHandler;
        private readonly INotificationService _notificationService;
        private readonly INotificationFactory _notificationFactory;
        private readonly IContactInfoRepository _contactInfoRepo;

        public SeniorService(
                ISeniorRepository repository,
                IMapper mapper,
                ILogger<SeniorService> logger,
                IOrderRepository orderRepository,
                OrderCancellationHandler orderCancellationHandler,
                INotificationService notificationService,
                INotificationFactory notificationFactory,
                IContactInfoRepository contactInfoRepo)
        {
                _repository = repository;
                _mapper = mapper;
                _logger = logger;
                _orderRepository = orderRepository;
                _orderCancellationHandler = orderCancellationHandler;
                _notificationService = notificationService;
                _notificationFactory = notificationFactory;
                _contactInfoRepo = contactInfoRepo;
        }

        public async Task<List<SeniorDto>> GetSeniorsByCustomerAsync(int customerId)
        {
                var seniors = await _repository.GetByCustomerIdAsync(customerId);

                return _mapper.Map<List<SeniorDto>>(seniors);
        }

        public async Task<SeniorDto> CreateSeniorAsync(SeniorCreateDto dto)
        {
                var senior = _mapper.Map<Senior>(dto);
                await _repository.AddAsync(senior);
                return _mapper.Map<SeniorDto>(senior);
        }


        public async Task<List<SeniorDto>> GetSeniorsWithExtraDetailsAsync(SeniorFilterDto? filter = null)
        {
                var seniors = await _repository.GetSeniorsWithExtraDetailsAsync(filter);
                return seniors; // Already mapped to dto using projection
        }


        public async Task<SeniorDto> GetBySeniorByIdAsync(int seniorId)
        {

                var senior = await _repository.GetByIdAsync(seniorId);
                return _mapper.Map<SeniorDto>(senior);
        }

        public async Task<bool> DeleteSeniorAsync(int seniorId)
        {
                _logger.LogInformation("🗑️ Deleting senior {SeniorId}", seniorId);

                try
                {
                        var senior = await _repository.GetByIdAsync(seniorId);
                        if (senior == null)
                        {
                                _logger.LogWarning("⚠️ Senior {SeniorId} not found", seniorId);
                                return false;
                        }

                        var originalName = $"Senior {seniorId}";

                        // Step 1: Cancel all orders that have active (non-cancelled) schedules
                        _logger.LogInformation("🔄 Cancelling orders for senior {SeniorId}", seniorId);
                        var orders = await _orderRepository.GetBySeniorAsync(seniorId);

                        foreach (var order in orders)
                        {
                                var hasActiveSchedules = order.Schedules.Any(s => !s.IsCancelled);
                                if (hasActiveSchedules)
                                {
                                        _logger.LogInformation("❌ Cancelling order {OrderId} for senior {SeniorId}", order.Id, seniorId);

                                        // Load with tracking for update
                                        var trackedOrder = await _orderRepository.LoadOrderWithIncludes(order.Id, new OrderIncludeOptions
                                        {
                                                Schedules = true,
                                                SchedulesJobRequests = true,
                                                ScheduleAssignments = true,
                                                AssignmentsJobInstances = true,
                                        }, asNoTracking: false);

                                        if (trackedOrder != null)
                                        {
                                                await _orderCancellationHandler.CancelOrderAsync(trackedOrder);
                                                await _orderRepository.UpdateAsync(trackedOrder);
                                        }
                                }
                        }
                        _logger.LogInformation("✅ Orders cancelled for senior {SeniorId}", seniorId);

                        // Step 2: Anonymize senior's contact info
                        if (senior.Contact != null)
                        {
                                _logger.LogInformation("🔐 Anonymizing contact info for senior {SeniorId}", seniorId);
                                await _contactInfoRepo.AnonymizeContactAsync(senior.Contact);
                        }

                        // Step 3: Send admin notification
                        try
                        {
                                _logger.LogInformation("📧 Sending deletion notification to admin for senior {SeniorId}", seniorId);
                                var notification = _notificationFactory.UserDeletedNotification(
                                        receiverUserId: 1, // admin
                                        deletedUserId: seniorId,
                                        deletedUserName: originalName,
                                        NotificationType.SeniorDeleted
                                );
                                await _notificationService.StoreAndNotifyAsync(notification);
                                _logger.LogInformation("✅ Admin notification sent for deleted senior {SeniorId}", seniorId);
                        }
                        catch (Exception notifyEx)
                        {
                                _logger.LogError(notifyEx, "⚠️ Failed to send deletion notification for senior {SeniorId}, but deletion completed", seniorId);
                        }

                        _logger.LogInformation("✅ Senior {SeniorId} deleted successfully", seniorId);
                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to delete senior {SeniorId}", seniorId);
                        return false;
                }
        }
}