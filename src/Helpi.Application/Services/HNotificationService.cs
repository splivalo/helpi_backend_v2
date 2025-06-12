using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class HNotificationService : IHNotificationService
{
    private readonly IHNotificationRepository _repository;
    private readonly IMapper _mapper;

    public HNotificationService(IHNotificationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<HNotificationDto?> GetByIdAsync(int id)
    {
        var notification = await _repository.GetByIdAsync(id);
        return notification != null ? _mapper.Map<HNotificationDto>(notification) : null;
    }

    public async Task<IEnumerable<HNotificationDto>> GetByUserIdAsync(int userId)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HNotificationDto>>(notifications);
    }

    public async Task<IEnumerable<HNotificationDto>> GetUnreadByUserIdAsync(int userId)
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

    public async Task<PagedHNotificationDto> GetPagedAsync(int userId, int page = 1, int pageSize = 10)
    {
        var notifications = await _repository.GetPagedAsync(userId, page, pageSize);
        var totalCount = (await _repository.GetByUserIdAsync(userId)).Count();

        return new PagedHNotificationDto
        {
            Notifications = _mapper.Map<IEnumerable<HNotificationDto>>(notifications),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}