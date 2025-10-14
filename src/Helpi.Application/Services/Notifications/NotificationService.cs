using System.Text.Json;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;


namespace Helpi.Application.Services;

public class NotificationService : INotificationService
{

    private readonly IFirebaseService _firebaseService;
    private readonly IFcmTokensRepository _fcmTokensRepository;

    private readonly IHNotificationRepository _hNotificationRepo;
    private readonly ISignalRNotificationService _signalRNotifier;

    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
         IFirebaseService firebaseService,
         IHNotificationRepository hNotificationRepo,
         ISignalRNotificationService signalRNotifier,
         IFcmTokensRepository fcmTokensRepository,
         ILogger<NotificationService> logger
        )
    {
        _hNotificationRepo = hNotificationRepo;
        _firebaseService = firebaseService;
        _signalRNotifier = signalRNotifier;
        _fcmTokensRepository = fcmTokensRepository;
        _logger = logger;
    }

    public async Task<bool> SendInAppNotificationAsync(int userId, HNotification notification)
    {
        throw new NotImplementedException();
    }

    private async Task<List<string>> GetUserDeviceFcmTokens(int userId)
    {
        var fcmTokens = await _fcmTokensRepository.GetTokensByUserIdAsync(userId);
        var deviceTokens = fcmTokens.Select(fcmToken => fcmToken.Token).ToList();
        return deviceTokens;
    }

    public async Task<bool> SendPushNotificationAsync(int userId, HNotification notification)
    {
        try
        {

            var deviceTokens = await GetUserDeviceFcmTokens(userId);
            if (!deviceTokens.Any())
            {
                _logger.LogInformation("❌ -- No FCM tokens found for userId: {userId}", userId);
                return false;
            }

            var hasSuccess = await _firebaseService.SendPushNotificationAsync(deviceTokens, notification);

            return hasSuccess;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "❌");
            return false;
        }
    }





    public async Task<bool> StoreAndNotifyAsync(HNotification notification, bool viaSignalR = true, bool viaFcm = false)
    {
        try
        {

            var hasSuccess = true;

            var userId = notification.RecieverUserId;

            // 1) store to db
            await _hNotificationRepo.CreateAsync(notification);

            /// should use a notification dto
            notification.Student = null;
            notification.Senior = null;


            // 2) send notification
            if (viaSignalR)
            {
                await _signalRNotifier.SendNotificationToUserAsync(userId, notification);
            }

            if (viaFcm)
            {
                await SendPushNotificationAsync(userId, notification);
            }

            return hasSuccess;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "❌");
            return false;
        }
    }

}