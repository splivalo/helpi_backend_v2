


using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;
public interface INotificationService
{
    Task<bool> SendPushNotificationAsync(int userId, HNotification notification);
    Task<bool> SendInAppNotificationAsync(int userId, HNotification notification);
    Task<bool> StoreNotificationAsync(HNotification notification);
}