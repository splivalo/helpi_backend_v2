using System.Text.Json;
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Utilities;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class HNotificationService : IHNotificationService
{
    private readonly IHNotificationRepository _repository;
    private readonly IMapper _mapper;

    private readonly ILocalizationService _localizer;

    public HNotificationService(IHNotificationRepository repository, IMapper mapper, ILocalizationService localizer)
    {
        _repository = repository;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<HNotificationDto?> GetByIdAsync(int id, string languageCode)
    {
        var notification = await _repository.GetByIdAsync(id);
        return notification != null ? _mapper.Map<HNotificationDto>(notification) : null;
    }

    public async Task<IEnumerable<HNotificationDto>> GetByUserIdAsync(int userId, string languageCode)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);
        var dtos = _mapper.Map<IEnumerable<HNotificationDto>>(notifications);

        return TranslateNotifications(dtos, languageCode);


    }

    public async Task<IEnumerable<HNotificationDto>> GetUnreadByUserIdAsync(int userId, string languageCode)
    {
        var notifications = await _repository.GetUnreadByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HNotificationDto>>(notifications);
    }

    public async Task<HNotificationDto> CreateAsync(CreateHNotificationDto createDto)
    {
        var notification = _mapper.Map<HNotification>(createDto);
        var createdNotification = await _repository.CreateAsync(notification);
        return _mapper.Map<HNotificationDto>(createdNotification);
    }

    public async Task<HNotificationDto?> UpdateAsync(int id, UpdateHNotificationDto updateDto)
    {
        var existingNotification = await _repository.GetByIdAsync(id);
        if (existingNotification == null)
            return null;

        _mapper.Map(updateDto, existingNotification);
        var updatedNotification = await _repository.UpdateAsync(existingNotification);
        return _mapper.Map<HNotificationDto>(updatedNotification);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var notification = await _repository.GetByIdAsync(id);
        if (notification == null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        return await _repository.MarkAsReadAsync(id);
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        return await _repository.MarkAllAsReadAsync(userId);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _repository.GetUnreadCountAsync(userId);
    }

    public async Task<PagedHNotificationDto> GetPagedAsync(int userId, int page = 1, int pageSize = 10, string languageCode = "en")
    {
        var notifications = await _repository.GetPagedAsync(userId, page, pageSize);
        var totalCount = (await _repository.GetByUserIdAsync(userId)).Count();

        var dtos = _mapper.Map<IEnumerable<HNotificationDto>>(notifications);

        return new PagedHNotificationDto
        {
            Notifications = TranslateNotifications(dtos, languageCode),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<IEnumerable<HNotificationDto>> GetReadByUserIdAsync(int userId, string languageCode)
    {
        var notifications = await _repository.GetReadByUserIdAsync(userId);
        var dtos = _mapper.Map<IEnumerable<HNotificationDto>>(notifications);
        return TranslateNotifications(dtos, languageCode);
    }

    public async Task<int> DeleteReadByUserIdAsync(int userId)
    {
        return await _repository.DeleteReadByUserIdAsync(userId);
    }

    private List<HNotificationDto> TranslateNotifications(IEnumerable<HNotificationDto> notifications, string languageCode)
    {
        var list = new List<HNotificationDto>();

        // User deleted types - need to parse Payload for format args
        var userDeletedList = new[]
        {
            NotificationType.StudentDeleted,
            NotificationType.SeniorDeleted,
            NotificationType.CustomerDeleted,
            NotificationType.AdminDeleted,
        };

        // Types whose body uses Senior name + Order: "{0}, Narudžba #{1}"
        var seniorAndOrderList = new[]
        {
            NotificationType.JobCancelled,
            NotificationType.JobReactivated,
            NotificationType.OrderCancelled,
            NotificationType.OrderScheduleCancelled,
            NotificationType.ScheduleAssignmentCancelled,
            NotificationType.NewOrderAdded,
        };

        // Types whose body is fully formatted in the factory — only translate Title
        var keepOriginalBodyList = new[]
        {
            NotificationType.JobRescheduled,
            NotificationType.AvailabilityChanged,
        };

        // Types whose Title AND Body are fully formatted — keep both as-is
        var keepOriginalTitleAndBodyList = new[]
        {
            NotificationType.AssignmentAccepted,
            NotificationType.AssignmentDeclined,
        };

        foreach (var dto in notifications)
        {
            // Skip title/body translation for types that arrive fully formatted
            if (keepOriginalTitleAndBodyList.Contains(dto.Type))
            {
                list.Add(dto);
                continue;
            }

            dto.Title = _localizer.GetString($"{dto.TranslationKey}.Title", languageCode);

            if (dto.Type == NotificationType.NewStudentAdded)
            {
                var studentName = dto.Student?.Contact?.FullName ?? "?";
                dto.Body = _localizer.GetString($"{dto.TranslationKey}.Body", languageCode, studentName);
            }
            else if (seniorAndOrderList.Contains(dto.Type))
            {
                var seniorName = dto.Senior?.Contact?.FullName ?? "?";
                var orderNumber = dto.OrderId ?? 0; // fallback to global ID
                if (!string.IsNullOrEmpty(dto.Payload))
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(dto.Payload);
                        if (payload.TryGetProperty("orderNumber", out var onProp) && onProp.TryGetInt32(out var on))
                            orderNumber = on;
                    }
                    catch { /* fallback to OrderId */ }
                }
                dto.Body = _localizer.GetString($"{dto.TranslationKey}.Body", languageCode, seniorName, orderNumber);
            }
            else if (dto.Type == NotificationType.NewSeniorAdded)
            {
                var seniorName = dto.Senior?.Contact?.FullName ?? "?";
                dto.Body = _localizer.GetString($"{dto.TranslationKey}.Body", languageCode, seniorName);
            }
            else if (userDeletedList.Contains(dto.Type))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<JsonElement>(dto.Payload ?? "{}");
                    var deletedUserName = payload.GetProperty("deletedUserName").GetString() ?? "";
                    var deletedUserId = payload.GetProperty("deletedUserId").GetInt32();

                    // Differentiated title per type
                    var titleKey = dto.Type switch
                    {
                        NotificationType.StudentDeleted => "Notifications.UserDeleted.StudentTitle",
                        NotificationType.SeniorDeleted => "Notifications.UserDeleted.SeniorTitle",
                        _ => $"{dto.TranslationKey}.Title"
                    };
                    dto.Title = _localizer.GetString(titleKey, languageCode);

                    dto.Body = _localizer.GetString($"{dto.TranslationKey}.Body", languageCode, deletedUserName, deletedUserId);
                }
                catch (Exception)
                {
                    // Keep original body on parse error
                }
            }
            else if (keepOriginalBodyList.Contains(dto.Type))
            {
                // Body already formatted by factory — keep it as-is
            }
            else
            {
                dto.Body = _localizer.GetString($"{dto.TranslationKey}.Body", languageCode);
            }


            list.Add(dto);
        }

        return list;
    }
}