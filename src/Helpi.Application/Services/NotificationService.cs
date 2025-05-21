using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Microsoft.Extensions.Logging;


namespace Helpi.Application.Services;
public class NotificationService : INotificationService
{

    private readonly IFirebaseService _firebaseService;
    private readonly IFcmTokensRepository _fcmTokensRepository;

    // private readonly ISignalRNotifier _signalRNotifier;

    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
         IFirebaseService firebaseService,
         IFcmTokensRepository fcmTokensRepository,
         ILogger<NotificationService> logger
        )
    {
        _firebaseService = firebaseService;
        _fcmTokensRepository = fcmTokensRepository;
        _logger = logger;
    }

    public async Task<bool> SendInAppNotificationAsync(int userId, HNotification notification)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SendPushNotificationAsync(int userId, HNotification notification)
    {
        try
        {
            var fcmTokens = await _fcmTokensRepository.GetTokensByUserIdAsync(userId);
            var deviceTokens = fcmTokens.Select(fcmToken => fcmToken.Token).ToList();

            var hasSuccess = await _firebaseService.SendPushNotificationAsync(deviceTokens, notification);

            return hasSuccess;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "❌");
            return false;
        }
    }

    public async Task<bool> StoreNotificationAsync(HNotification notification)
    {
        throw new NotImplementedException();
    }

}