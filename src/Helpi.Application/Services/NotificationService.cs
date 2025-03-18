using Helpi.Application.Interfaces.Services;


namespace Helpi.Application.Services;
public class NotificationService : INotificationService
{
    public async Task<bool> SendJobRequestNotification(int userId, int orderScheduleId, DateTime expiresAt)
    {

        // throw new NotImplementedException();
        return await Task.FromResult(true);
    }
}