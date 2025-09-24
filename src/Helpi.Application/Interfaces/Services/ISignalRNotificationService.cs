

using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;

public interface ISignalRNotificationService
{
    Task SendNotificationToUserAsync(int userId, HNotification notification);
    Task SendNotificationToGroupAsync(string groupName, HNotification notification);
    Task SendUnreadCountUpdateAsync(int userId, int unreadCount);
    Task SendTypingIndicatorAsync(int userId, string message);
    Task BroadcastSystemNotificationAsync(HNotification notification);
}