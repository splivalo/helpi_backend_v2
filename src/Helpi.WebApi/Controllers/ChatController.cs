using System.Security.Claims;
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Helpi.WebApi.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public ChatController(
        ChatService chatService,
        IHubContext<ChatHub> chatHub,
        IHubContext<NotificationHub> notificationHub)
    {
        _chatService = chatService;
        _chatHub = chatHub;
        _notificationHub = notificationHub;
    }

    /// <summary>
    /// Get all chat rooms for the authenticated user.
    /// </summary>
    [HttpGet("rooms")]
    public async Task<ActionResult<List<ChatRoomDto>>> GetRooms()
    {
        var userId = GetUserId();
        var rooms = await _chatService.GetUserRoomsAsync(userId);
        return Ok(rooms);
    }

    /// <summary>
    /// Get or create a chat room with another user.
    /// </summary>
    [HttpPost("rooms")]
    public async Task<ActionResult<ChatRoomDto>> CreateRoom([FromBody] CreateChatRoomDto dto)
    {
        var userId = GetUserId();
        if (dto.OtherUserId == userId)
            return BadRequest("Cannot create a chat room with yourself");

        var room = await _chatService.GetOrCreateRoomAsync(userId, dto.OtherUserId);
        return Ok(room);
    }

    /// <summary>
    /// Get paginated messages for a chat room.
    /// </summary>
    [HttpGet("rooms/{roomId}/messages")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        int roomId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        var messages = await _chatService.GetMessagesAsync(roomId, userId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Send a message to a chat room (REST fallback — prefer SignalR hub for real-time).
    /// </summary>
    [HttpPost("rooms/{roomId}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        int roomId,
        [FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var message = await _chatService.SendMessageAsync(roomId, userId, dto.Content);

        // Broadcast via ChatHub to users in the room
        await _chatHub.Clients.Group($"chat_room_{roomId}")
            .SendAsync("ReceiveChatMessage", message);

        // Notify other participant via both hubs (clients connect to /hubs/notifications)
        var rooms = await _chatService.GetUserRoomsAsync(userId);
        var room = rooms.FirstOrDefault(r => r.Id == roomId);
        if (room != null)
        {
            var otherUserId = room.Participant1UserId == userId
                ? room.Participant2UserId
                : room.Participant1UserId;
            await _chatHub.Clients.Group($"chat_user_{otherUserId}")
                .SendAsync("ChatUnreadUpdate", new { roomId, message });

            // Also send via NotificationHub so both apps receive it
            await _notificationHub.Clients.Group($"user_{otherUserId}")
                .SendAsync("ReceiveChatMessage", message);
        }

        return Ok(message);
    }

    /// <summary>
    /// Mark all messages in a room as read for the authenticated user.
    /// </summary>
    [HttpPut("rooms/{roomId}/read")]
    public async Task<ActionResult> MarkAsRead(int roomId)
    {
        var userId = GetUserId();
        var count = await _chatService.MarkAsReadAsync(roomId, userId);

        // Broadcast read receipt via ChatHub
        await _chatHub.Clients.Group($"chat_room_{roomId}")
            .SendAsync("ChatMessagesRead", new { roomId, readByUserId = userId, readAt = DateTime.UtcNow });

        return Ok(new { markedAsRead = count });
    }

    /// <summary>
    /// Get total unread message count across all rooms.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _chatService.GetUnreadCountAsync(userId);
        return Ok(new { unreadCount = count });
    }

    private int GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");
        return userId;
    }
}
