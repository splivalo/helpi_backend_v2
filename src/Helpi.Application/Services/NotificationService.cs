using Helpi.Application.Interfaces.Services;


namespace Helpi.Application.Services;
public class NotificationService : INotificationService
{
    public Task<bool> SendJobRequestNotification(int userId, int orderScheduleId, DateTime expiresAt)
    {
        throw new NotImplementedException();
    }
}