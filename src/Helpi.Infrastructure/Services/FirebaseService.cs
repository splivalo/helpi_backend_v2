using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Services;



public class FirebaseService : IFirebaseService
{
    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly ILogger<FirebaseService> _logger;
    public FirebaseService(
        ILogger<FirebaseService> logger
    )
    {
        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        _logger = logger;
    }

    public async Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, object>? claims)
    {
        return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(userId, claims);
    }


    public async Task<bool> SendPushNotificationAsync(List<string> deviceTokens, HNotification notification)
    {

        var message = new MulticastMessage
        {
            Tokens = deviceTokens,
            Data = new Dictionary<string, string>
            {
                ["notificationId"] = notification.Id.ToString(),
                ["type"] = notification.Type.ToString(),
                ["payload"] = notification.Payload ?? string.Empty
            },
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body
            }
        };

        var response = await _firebaseMessaging.SendEachForMulticastAsync(message);
        var failCount = LogFailedAttempts(response, notification.RecieverUserId);

        return failCount < response.Responses.Count;
    }

    private int LogFailedAttempts(BatchResponse response, int userId)
    {
        var failCount = 0;

        for (var i = 0; i < response.Responses.Count; i++)
        {
            if (!response.Responses[i].IsSuccess)
            {
                failCount = failCount + 1;

                _logger.LogError(
                    "❌ FCM Failed to send notification to user {UserId}: {Error}",
                    userId,
                    response.Responses[i].Exception?.Message);
            }
        }

        return failCount;
    }

}