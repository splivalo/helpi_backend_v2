using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IHNotificationRepository
{
    Task<HNotification?> GetByIdAsync(int id);
    Task<IEnumerable<HNotification>> GetByUserIdAsync(int userId);
    Task<IEnumerable<HNotification>> GetUnreadByUserIdAsync(int userId);
    Task<HNotification> CreateAsync(HNotification notification);
    Task<HNotification> UpdateAsync(HNotification notification);
    Task DeleteAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<IEnumerable<HNotification>> GetPagedAsync(int userId, int page, int pageSize);
}