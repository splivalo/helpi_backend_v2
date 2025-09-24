using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface IHNotificationService
{
    Task<HNotificationDto?> GetByIdAsync(int id);
    Task<IEnumerable<HNotificationDto>> GetByUserIdAsync(int userId);
    Task<IEnumerable<HNotificationDto>> GetUnreadByUserIdAsync(int userId);
    Task<HNotificationDto> CreateAsync(CreateHNotificationDto createDto);
    Task<HNotificationDto?> UpdateAsync(int id, UpdateHNotificationDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<PagedHNotificationDto> GetPagedAsync(int userId, int page = 1, int pageSize = 10);
}