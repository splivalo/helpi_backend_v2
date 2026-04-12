using System.Security.Claims;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Helpi.WebApi.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatService _chatService;

    public ChatHub(ChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific chat room group to receive real-time messages.
    /// </summary>
    public async Task JoinRoom(int roomId)
    {
        var userId = GetUserId();
        // Validate the user is a participant (GetMessagesAsync throws if not)
        await _chatService.GetMessagesAsync(roomId, userId, 1, 1);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_room_{roomId}");
    }

    /// <summary>
    /// Leave a chat room group.
    /// </summary>
    public async Task LeaveRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_room_{roomId}");
    }

    /// <summary>
    /// Send a message to a chat room. Broadcasts to all room members.
    /// </summary>
    public async Task SendMessage(int roomId, string content)
    {
        var userId = GetUserId();
        var messageDto = await _chatService.SendMessageAsync(roomId, userId, content);

        // Broadcast to everyone in the room (including sender for confirmation)
        await Clients.Group($"chat_room_{roomId}")
            .SendAsync("ReceiveChatMessage", messageDto);

        // Also notify the other user's global chat group (for unread badge updates)
        var rooms = await _chatService.GetMessagesAsync(roomId, userId, 1, 1);
        var room = await _chatService.GetUserRoomsAsync(userId);
        var targetRoom = room.FirstOrDefault(r => r.Id == roomId);
        if (targetRoom != null)
        {
            var otherUserId = targetRoom.Participant1UserId == userId
                ? targetRoom.Participant2UserId
                : targetRoom.Participant1UserId;
            await Clients.Group($"chat_user_{otherUserId}")
                .SendAsync("ChatUnreadUpdate", new { roomId, messageDto });
        }
    }

    /// <summary>
    /// Mark all messages in a room as read.
    /// </summary>
    public async Task MarkRead(int roomId)
    {
        var userId = GetUserId();
        await _chatService.MarkAsReadAsync(roomId, userId);

        // Notify the room that messages were read
        await Clients.Group($"chat_room_{roomId}")
            .SendAsync("ChatMessagesRead", new { roomId, readByUserId = userId, readAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Send typing indicator to a room.
    /// </summary>
    public async Task SendTyping(int roomId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"chat_room_{roomId}")
            .SendAsync("ChatTyping", new { roomId, userId });
    }

    private int GetUserId()
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            throw new HubException("User not authenticated");
        return userId;
    }
}
