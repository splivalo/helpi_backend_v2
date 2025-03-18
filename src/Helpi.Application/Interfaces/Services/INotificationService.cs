


namespace Helpi.Application.Interfaces.Services;
public interface INotificationService
{
    Task<bool> SendJobRequestNotification(int userId, int orderScheduleId, DateTime expiresAt);
}