using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
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


    public async Task<bool> SendPushNotificationAsync(List<FcmToken> deviceTokens, HNotificationDto notification)
    {

        var androidTokens = deviceTokens
         .Where(d => d.Platform == DevicePlatform.Android)
         .Select(d => d.Token)
         .ToList();

        var iosTokens = deviceTokens
            .Where(d => d.Platform == DevicePlatform.iOS)
            .Select(d => d.Token)
            .ToList();

        var results = new List<BatchResponse>();

        if (androidTokens.Count > 0)
        {
            results.Add(await SendAndroidAsync(androidTokens, notification));
        }

        if (iosTokens.Count > 0)
        {
            results.Add(await SendIosAsync(iosTokens, notification));
        }

        return results.Any(r => r.SuccessCount > 0);

    }

    private async Task<BatchResponse> SendAndroidAsync(
    List<string> tokens,
    HNotificationDto n)
    {
        var message = new MulticastMessage
        {
            Tokens = tokens,
            Data = new Dictionary<string, string>
            {
                ["title"] = n.Title,
                ["body"] = n.Body,
                ["notificationId"] = n.Id.ToString(),
                ["type"] = n.Type.ToString(),
                ["payload"] = n.Payload ?? string.Empty
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High
            }
        };

        var response = await _firebaseMessaging.SendEachForMulticastAsync(message);
        LogFailedAttempts(response, n.RecieverUserId);

        return response;
    }

    private async Task<BatchResponse> SendIosAsync(
    List<string> tokens,
    HNotificationDto n)
    {
        var message = new MulticastMessage
        {
            Tokens = tokens,

            Notification = new Notification
            {
                Title = n.Title,
                Body = n.Body
            },

            Data = new Dictionary<string, string>
            {
                ["title"] = n.Title,
                ["body"] = n.Body,
                ["notificationId"] = n.Id.ToString(),
                ["type"] = n.Type.ToString(),
                ["payload"] = n.Payload ?? string.Empty
            },

            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    ["apns-priority"] = "10"
                },
                Aps = new Aps
                {
                    Sound = "default",
                    Badge = 1
                }
            }
        };

        var response = await _firebaseMessaging.SendEachForMulticastAsync(message);
        LogFailedAttempts(response, n.RecieverUserId);

        return response;
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