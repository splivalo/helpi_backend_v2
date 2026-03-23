using System.Text.Json;
using AutoMapper;
using Helpi.Application.DTOs;
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

    private readonly IMapper _mapper;

    public NotificationService(
  IMapper mapper,
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
        _mapper = mapper;
    }

    public async Task<bool> SendInAppNotificationAsync(int userId, HNotification notification)
    {
        throw new NotImplementedException();
    }

    private async Task<List<FcmToken>> GetUserDeviceFcmTokens(int userId)
    {
        var fcmTokens = await _fcmTokensRepository.GetTokensByUserIdAsync(userId);
        // var deviceTokens = fcmTokens.Select(fcmToken => fcmToken.Token).ToList();
        return fcmTokens;
    }

    public async Task<bool> SendNotificationAsync(int userId,
     HNotification notification,
     bool viaSignalR = true,
      bool viaFcm = true)
    {

        try
        {
            var hasSuccess = true;

            if (viaSignalR)
            {
                var notificationDto = _mapper.Map<HNotificationDto>(notification);
                await _signalRNotifier.SendNotificationToUserAsync(userId, notificationDto);
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

    public async Task<bool> SendPushNotificationAsync(int userId, HNotification notification)
    {
        try
        {

            var notificationDto = _mapper.Map<HNotificationDto>(notification);

            var deviceTokens = await GetUserDeviceFcmTokens(userId);
            if (!deviceTokens.Any())
            {
                _logger.LogInformation("❌ -- No FCM tokens found for userId: {userId}", userId);
                return false;
            }

            var hasSuccess = await _firebaseService.SendPushNotificationAsync(deviceTokens, notificationDto);

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



            // 2) send notification
            if (viaSignalR)
            {
                var notificationDto = _mapper.Map<HNotificationDto>(notification);
                await _signalRNotifier.SendNotificationToUserAsync(userId, notificationDto);
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

    public async Task StoreAndNotifyAdminsAsync(List<int> adminIds, Func<int, HNotification> notificationBuilder)
    {
        foreach (var adminId in adminIds)
        {
            try
            {
                var notification = notificationBuilder(adminId);
                await StoreAndNotifyAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to notify admin {AdminId}", adminId);
            }
        }
    }

}