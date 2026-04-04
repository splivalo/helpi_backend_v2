

using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface ISignalRNotificationService
{
    Task SendNotificationToUserAsync(int userId, HNotificationDto notification);
    Task SendNotificationToGroupAsync(string groupName, HNotificationDto notification);
    Task SendUnreadCountUpdateAsync(int userId, int unreadCount);
    Task SendTypingIndicatorAsync(int userId, string message);
    Task BroadcastSystemNotificationAsync(HNotificationDto notification);
    Task BroadcastSettingsChangedAsync();
}