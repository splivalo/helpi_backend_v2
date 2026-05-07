
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
        private readonly IJobInstanceRepository _jobInstanceRepo;
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepo;

        public SeniorService(
                ISeniorRepository repository,
                IMapper mapper,
                ILogger<SeniorService> logger,
                IOrderRepository orderRepository,
                OrderCancellationHandler orderCancellationHandler,
                INotificationService notificationService,
                INotificationFactory notificationFactory,
                IContactInfoRepository contactInfoRepo,
                IJobInstanceRepository jobInstanceRepo,
                IUserRepository userRepository,
                ICustomerRepository customerRepo)
        {
                _repository = repository;
                _mapper = mapper;
                _logger = logger;
                _orderRepository = orderRepository;
                _orderCancellationHandler = orderCancellationHandler;
                _notificationService = notificationService;
                _notificationFactory = notificationFactory;
                _contactInfoRepo = contactInfoRepo;
                _jobInstanceRepo = jobInstanceRepo;
                _userRepository = userRepository;
                _customerRepo = customerRepo;
        }

        public async Task<List<SeniorDto>> GetSeniorsByCustomerAsync(int customerId)
        {
                var seniors = await _repository.GetByCustomerIdAsync(customerId);

                return _mapper.Map<List<SeniorDto>>(seniors);
        }

        public async Task<SeniorDto> UpdateSeniorAsync(int seniorId, SeniorUpdateDto dto)
        {
                var senior = await _repository.GetByIdAsync(seniorId)
                        ?? throw new KeyNotFoundException($"Senior {seniorId} not found.");

                var oldRelationship = senior.Relationship;
                var newRelationship = dto.Relationship ?? oldRelationship;

                // Self → non-Self: split contacts (create a NEW contact for the senior,
                // keep the original on the Customer side as orderer)
                if (oldRelationship == Relationship.Self && newRelationship != Relationship.Self)
                {
                        var original = await _contactInfoRepo.GetByIdAsync(senior.ContactId)
                                ?? throw new InvalidOperationException("Original contact not found.");

                        var newContact = new ContactInfo
                        {
                                FullName = original.FullName,
                                DateOfBirth = original.DateOfBirth,
                                Phone = original.Phone,
                                Email = original.Email,
                                LanguageCode = original.LanguageCode,
                                Gender = original.Gender,
                                GooglePlaceId = original.GooglePlaceId,
                                FullAddress = original.FullAddress,
                                CityId = original.CityId,
                                CityName = original.CityName,
                                Latitude = original.Latitude,
                                Longitude = original.Longitude,
                                PostalCode = original.PostalCode,
                                Country = original.Country,
                                State = original.State,
                        };
                        var created = await _contactInfoRepo.AddAsync(newContact);
                        senior.ContactId = created.Id;
                }
                // Non-Self → Self: merge (delete the senior's separate contact, point to Customer's)
                else if (oldRelationship != Relationship.Self && newRelationship == Relationship.Self)
                {
                        var customer = await _customerRepo.GetByIdAsync(senior.CustomerId)
                                ?? throw new InvalidOperationException("Customer not found for senior.");
                        var oldContactId = senior.ContactId;

                        // Point senior to customer's contact
                        senior.ContactId = customer.ContactId;

                        // Clean up the now-orphaned contact
                        if (oldContactId != customer.ContactId)
                        {
                                var orphan = await _contactInfoRepo.GetByIdAsync(oldContactId);
                                if (orphan != null) await _contactInfoRepo.DeleteAsync(orphan);
                        }
                }

                senior.Relationship = newRelationship;

                if (dto.SpecialRequirements != null)
                        senior.SpecialRequirements = dto.SpecialRequirements;

                await _repository.UpdateAsync(senior);

                _logger.LogInformation("Senior {SeniorId} updated (Relationship: {Old} → {New})",
                        seniorId, oldRelationship, newRelationship);

                return await GetBySeniorByIdAsync(seniorId);
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
                if (senior == null)
                {
                        throw new InvalidOperationException($"Senior {seniorId} not found.");
                }

                var dto = _mapper.Map<SeniorDto>(senior);
                // Senior.CustomerId == Customer.UserId == User.Id
                var user = await _userRepository.GetByIdAsync(senior.CustomerId);
                dto.IsSuspended = user?.IsSuspended ?? false;
                dto.SuspensionReason = user?.SuspensionReason;

                // Populate orderer contact when Relationship != Self
                if (senior.Relationship != Relationship.Self)
                {
                        var customer = await _customerRepo.GetByIdAsync(senior.CustomerId);
                        if (customer != null)
                        {
                                var ordererContact = await _contactInfoRepo.GetByIdAsync(customer.ContactId);
                                if (ordererContact != null)
                                {
                                        dto.OrdererContact = _mapper.Map<ContactInfoDto>(ordererContact);
                                }
                        }
                }

                return dto;
        }

        public async Task<bool> DeleteSeniorAsync(int seniorId)
        {
                _logger.LogInformation("Deleting senior {SeniorId}", seniorId);

                try
                {
                        var senior = await GetSeniorForDeletionAsync(seniorId);
                        if (senior == null)
                        {
                                return false;
                        }

                        var originalName = $"Senior {seniorId}";

                        var affectedAssignments = await CancelOrdersAndCollectAffectedStudentsAsync(seniorId);

                        await AnonymizeSeniorContactAsync(senior);

                        await SoftDeleteSeniorAsync(senior);

                        await NotifyAdminOfDeletionAsync(seniorId, originalName);

                        await NotifyAffectedStudentsAsync(affectedAssignments, seniorId);

                        _logger.LogInformation("Senior {SeniorId} deleted successfully", seniorId);
                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Failed to delete senior {SeniorId}", seniorId);
                        return false;
                }
        }

        #region DeleteSeniorAsync Helper Methods

        private async Task<Senior?> GetSeniorForDeletionAsync(int seniorId)
        {
                var senior = await _repository.GetByIdAsync(seniorId);
                if (senior == null)
                {
                        _logger.LogWarning("Senior {SeniorId} not found", seniorId);
                }
                return senior;
        }

        private async Task<List<ScheduleAssignment>> CancelOrdersAndCollectAffectedStudentsAsync(int seniorId)
        {
                _logger.LogInformation("Cancelling orders for senior {SeniorId}", seniorId);

                var affectedAssignments = new List<ScheduleAssignment>();
                var orders = await _orderRepository.GetBySeniorAsync(seniorId);

                foreach (var order in orders)
                {
                        var hasActiveSchedules = order.Schedules.Any(s => !s.IsCancelled);
                        if (hasActiveSchedules)
                        {
                                _logger.LogInformation("Cancelling order {OrderId} for senior {SeniorId}", order.Id, seniorId);

                                var trackedOrder = await _orderRepository.LoadOrderWithIncludes(order.Id, new OrderIncludeOptions
                                {
                                        Schedules = true,
                                        SchedulesJobRequests = true,
                                        ScheduleAssignments = true,
                                        ScheduleAssignmentStudent = true,
                                        AssignmentsJobInstances = true,
                                }, asNoTracking: false);

                                if (trackedOrder != null)
                                {
                                        var activeAssignments = trackedOrder.Schedules
                                                .SelectMany(s => s.Assignments)
                                                .Where(a => !a.IsTerminal)
                                                .ToList();

                                        affectedAssignments.AddRange(activeAssignments);

                                        await _orderCancellationHandler.CancelOrderAsync(trackedOrder);
                                        await _orderRepository.UpdateAsync(trackedOrder);
                                }
                        }
                }

                _logger.LogInformation("Orders cancelled for senior {SeniorId}. Affected students: {Count}", seniorId, affectedAssignments.Count);
                return affectedAssignments;
        }

        private async Task AnonymizeSeniorContactAsync(Senior senior)
        {
                if (senior.Contact != null)
                {
                        _logger.LogInformation("Anonymizing contact info for senior {SeniorId}", senior.Id);
                        await _contactInfoRepo.AnonymizeContactAsync(senior.Contact);
                }
        }

        private async Task SoftDeleteSeniorAsync(Senior senior)
        {
                senior.DeletedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(senior);
                _logger.LogInformation("Senior {SeniorId} soft deleted successfully", senior.Id);
        }

        private async Task NotifyAdminOfDeletionAsync(int seniorId, string originalName)
        {
                try
                {
                        _logger.LogInformation("Sending deletion notification to admin for senior {SeniorId}", seniorId);
                        var notification = _notificationFactory.UserDeletedNotification(
                                receiverUserId: 1, // admin
                                deletedUserId: seniorId,
                                deletedUserName: originalName,
                                NotificationType.SeniorDeleted
                        );
                        await _notificationService.StoreAndNotifyAsync(notification);
                        _logger.LogInformation("Admin notification sent for deleted senior {SeniorId}", seniorId);
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Failed to send deletion notification for senior {SeniorId}, but deletion completed", seniorId);
                }
        }

        private async Task NotifyAffectedStudentsAsync(List<ScheduleAssignment> assignments, int seniorId)
        {
                if (assignments.Count == 0)
                {
                        return;
                }

                _logger.LogInformation("Notifying affected students for senior {SeniorId} deletion", seniorId);

                // One notification per (student, order) — a student assigned to 9 schedules
                // in the same order should receive only one cancellation notification.
                var notifiedKeys = new HashSet<(int StudentId, int OrderId)>();

                foreach (var assignment in assignments)
                {
                        var key = (assignment.StudentId, assignment.OrderId);
                        if (!notifiedKeys.Add(key))
                                continue;

                        try
                        {
                                var culture = assignment.Student?.Contact?.LanguageCode ?? "hr";
                                var notification = _notificationFactory.ScheduleAssignmentCancelledNotification(
                                        recieverId: assignment.StudentId,
                                        scheduleAssignment: assignment,
                                        seniorId: seniorId,
                                        culture: culture
                                );
                                await _notificationService.StoreAndNotifyAsync(notification);
                                _logger.LogInformation("Notified student {StudentId} of assignment cancellation for order {OrderId}", assignment.StudentId, assignment.OrderId);
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "Failed to notify student {StudentId}, continuing with deletion", assignment.StudentId);
                        }
                }
        }

        #endregion

        #region Archive Methods

        /// <summary>
        /// Checks if senior can be archived and returns blocking item counts.
        /// </summary>
        public async Task<ArchiveCheckDto> GetArchiveCheckAsync(int seniorId)
        {
                var orders = await _orderRepository.GetBySeniorAsync(seniorId);

                // Count active orders (not cancelled, not completed)
                var activeOrders = orders.Where(o =>
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.Completed).ToList();

                var activeOrdersCount = activeOrders.Count;

                // Count upcoming sessions
                var upcomingSessions = await _jobInstanceRepo.GetJobInstancesAsync(
                        assignmentId: null,
                        prevAssignmentId: null,
                        status: JobInstanceStatus.Upcoming,
                        new SessionIncludeOptions());

                var seniorUpcomingSessions = upcomingSessions.Where(j => j.SeniorId == seniorId).ToList();
                var upcomingCount = seniorUpcomingSessions.Count;

                var hasBlocking = activeOrdersCount > 0 || upcomingCount > 0;

                return new ArchiveCheckDto
                {
                        CanArchiveDirectly = !hasBlocking,
                        HasBlockingItems = hasBlocking,
                        ActiveOrdersCount = activeOrdersCount,
                        UpcomingSessionsCount = upcomingCount,
                        Message = hasBlocking
                                ? $"Senior ima {activeOrdersCount} aktivnih narudžbi i {upcomingCount} nadolazećih termina. Sve će biti otkazano."
                                : "Senior nema aktivnih narudžbi."
                };
        }

        /// <summary>
        /// Archives a senior. If force=true, cancels all orders and sessions first.
        /// </summary>
        public async Task<ArchiveResultDto> ArchiveSeniorAsync(int seniorId, ArchiveRequestDto request)
        {
                _logger.LogInformation("📦 Archiving senior {SeniorId}, Force={Force}", seniorId, request.Force);

                var senior = await _repository.GetByIdAsync(seniorId);
                if (senior == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Senior not found" };
                }

                var check = await GetArchiveCheckAsync(seniorId);

                if (check.HasBlockingItems && !request.Force)
                {
                        return new ArchiveResultDto
                        {
                                Success = false,
                                Message = check.Message
                        };
                }

                var cancelledOrdersCount = 0;
                var cancelledSessionsCount = 0;

                // If force, cancel all active orders
                if (check.HasBlockingItems && request.Force)
                {
                        _logger.LogInformation("🔄 Force archiving - cancelling {Count} active orders", check.ActiveOrdersCount);

                        var affectedAssignments = await CancelOrdersAndCollectAffectedStudentsAsync(seniorId);
                        await NotifyAffectedStudentsAsync(affectedAssignments, seniorId);

                        cancelledOrdersCount = check.ActiveOrdersCount;
                        cancelledSessionsCount = check.UpcomingSessionsCount;
                }

                // Archive the senior (soft delete)
                senior.DeletedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(senior);

                _logger.LogInformation("✅ Senior {SeniorId} archived successfully", seniorId);

                return new ArchiveResultDto
                {
                        Success = true,
                        Message = "Senior uspješno arhiviran",
                        CancelledOrdersCount = cancelledOrdersCount,
                        CancelledSessionsCount = cancelledSessionsCount
                };
        }

        /// <summary>
        /// Unarchives a senior by clearing the DeletedAt timestamp.
        /// </summary>
        public async Task<ArchiveResultDto> UnarchiveSeniorAsync(int seniorId)
        {
                _logger.LogInformation("📦 Unarchiving senior {SeniorId}", seniorId);

                var senior = await _repository.GetByIdIncludingArchivedAsync(seniorId);
                if (senior == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Senior not found" };
                }

                if (senior.DeletedAt == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Senior is not archived" };
                }

                senior.DeletedAt = null;
                await _repository.UpdateAsync(senior);

                _logger.LogInformation("✅ Senior {SeniorId} unarchived successfully", seniorId);

                return new ArchiveResultDto
                {
                        Success = true,
                        Message = "Senior uspješno vraćen iz arhive"
                };
        }

        #endregion
}