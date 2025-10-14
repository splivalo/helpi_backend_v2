


using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;

public interface INotificationService
{
    Task<bool> SendPushNotificationAsync(int userId, HNotification notification);
    Task<bool> SendInAppNotificationAsync(int userId, HNotification notification);

    Task<bool> StoreAndNotifyAsync(HNotification notification, bool viaSignalR = true, bool viaFcm = false);

}