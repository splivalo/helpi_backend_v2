
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;
public interface IFirebaseService
{
    public Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, dynamic>? claims);

    public Task<bool> SendPushNotificationAsync(List<string> deviceTokens, HNotification notification);
}