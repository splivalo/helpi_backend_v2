using Helpi.Application.DTOs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace Helpi.WebAPI.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(int userId, HNotificationDto notification)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToGroupAsync(string groupName, HNotificationDto notification)
    {
        await _hubContext.Clients.Group(groupName)
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendUnreadCountUpdateAsync(int userId, int unreadCount)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("UnreadCountUpdate", unreadCount);
    }

    public async Task SendTypingIndicatorAsync(int userId, string message)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("TypingIndicator", message);
    }

    public async Task BroadcastSystemNotificationAsync(HNotificationDto notification)
    {
        await _hubContext.Clients.All
            .SendAsync("SystemNotification", notification);
    }
}
