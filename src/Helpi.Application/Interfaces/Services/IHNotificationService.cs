using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface IHNotificationService
{
    Task<HNotificationDto?> GetByIdAsync(int id, string languageCode);
    Task<IEnumerable<HNotificationDto>> GetByUserIdAsync(int userId, string languageCode);
    Task<IEnumerable<HNotificationDto>> GetUnreadByUserIdAsync(int userId, string languageCode);
    Task<HNotificationDto> CreateAsync(CreateHNotificationDto createDto);
    Task<HNotificationDto?> UpdateAsync(int id, UpdateHNotificationDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<PagedHNotificationDto> GetPagedAsync(int userId, int page = 1, int pageSize = 10, string languageCode = "en");
    Task<IEnumerable<HNotificationDto>> GetReadByUserIdAsync(int userId, string languageCode);
    Task<int> DeleteReadByUserIdAsync(int userId);
}