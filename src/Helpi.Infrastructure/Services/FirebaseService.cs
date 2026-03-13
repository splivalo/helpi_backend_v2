using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Services;



public class FirebaseService : IFirebaseService
{
    private readonly FirebaseMessaging? _firebaseMessaging;
    private readonly ILogger<FirebaseService> _logger;
    private readonly FirestoreDb? _firestoreDb;



    public FirebaseService(
        ILogger<FirebaseService> logger
    )
    {
        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        _logger = logger;

        if (_firebaseMessaging == null)
        {
            _logger.LogWarning("⚠️ FirebaseMessaging not available — push notifications will be skipped.");
        }

        // Initialize Firestore using the already-initialized Firebase app
        try
        {
            var projectId = FirebaseApp.DefaultInstance?.Options?.ProjectId;
            if (!string.IsNullOrEmpty(projectId))
            {
                _firestoreDb = FirestoreDb.Create(projectId);
                _logger.LogInformation("🔥 Firestore initialized with project: {ProjectId}", projectId);
            }
            else
            {
                _logger.LogWarning("⚠️ Firebase project ID not found. Firestore operations will be skipped.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize Firestore. Firestore operations will be skipped.");
        }
    }

    public async Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, object>? claims)
    {
        if (FirebaseAuth.DefaultInstance == null)
        {
            _logger.LogWarning("⚠️ Firebase not initialized — returning empty token for user {UserId}", userId);
            return string.Empty;
        }
        return await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(userId, claims);
    }

    public async Task AnonymizeAndLogoutUserAsync(int backendUserId)
    {
        var firebaseUid = backendUserId.ToString();
        _logger.LogInformation("🔥 Starting Firebase anonymization and logout for user {UserId} (Firebase UID: {FirebaseUid})", backendUserId, firebaseUid);

        // Step 1: Revoke Firebase refresh tokens (logs out all sessions)
        try
        {
            await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(firebaseUid);
            _logger.LogInformation("✅ Successfully revoked Firebase refresh tokens for user {UserId}", backendUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to revoke Firebase refresh tokens for user {UserId}. Continuing with Firestore deletion.", backendUserId);
            // Don't throw - continue with Firestore deletion
        }

        // Step 2: Delete user document from Firestore
        if (_firestoreDb == null)
        {
            _logger.LogWarning("⚠️ Firestore not initialized. Skipping user document deletion for user {UserId}", backendUserId);
            return;
        }

        try
        {
            var userDocRef = _firestoreDb.Collection("users").Document(firebaseUid);
            await userDocRef.DeleteAsync();
            _logger.LogInformation("✅ Successfully deleted Firestore user document for user {UserId}", backendUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete Firestore user document for user {UserId}", backendUserId);
            // Don't throw - we've done our best effort, DB deletion should continue
        }

        _logger.LogInformation("🔥 Completed Firebase anonymization and logout for user {UserId}", backendUserId);
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
                    // ContentAvailable = true,
                    // MutableContent = true,
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