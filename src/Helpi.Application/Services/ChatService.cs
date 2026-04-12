using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class ChatService
{
    private readonly IChatRepository _chatRepo;
    private readonly IUserRepository _userRepo;
    private readonly ISignalRNotificationService _signalR;
    private readonly IMapper _mapper;

    public ChatService(
        IChatRepository chatRepo,
        IUserRepository userRepo,
        ISignalRNotificationService signalR,
        IMapper mapper)
    {
        _chatRepo = chatRepo;
        _userRepo = userRepo;
        _signalR = signalR;
        _mapper = mapper;
    }

    /// <summary>
    /// Get or create a chat room between current user and other user.
    /// </summary>
    public async Task<ChatRoomDto> GetOrCreateRoomAsync(int currentUserId, int otherUserId)
    {
        // Check if room already exists
        var room = await _chatRepo.GetRoomByParticipantsAsync(currentUserId, otherUserId);
        if (room != null)
            return MapRoomToDto(room, currentUserId);

        // Load both users to get names and roles
        var currentUser = await _userRepo.GetByIdAsync(currentUserId)
            ?? throw new KeyNotFoundException($"User {currentUserId} not found");
        var otherUser = await _userRepo.GetByIdAsync(otherUserId)
            ?? throw new KeyNotFoundException($"User {otherUserId} not found");

        room = new ChatRoom
        {
            Participant1UserId = currentUserId,
            Participant1Name = await GetUserDisplayNameAsync(currentUser),
            Participant1Role = currentUser.UserType.ToString().ToLower(),
            Participant2UserId = otherUserId,
            Participant2Name = await GetUserDisplayNameAsync(otherUser),
            Participant2Role = otherUser.UserType.ToString().ToLower(),
        };

        room = await _chatRepo.CreateRoomAsync(room);

        // Send welcome message from admin when room is first created
        var adminUserId = otherUser.UserType == UserType.Admin ? otherUserId : currentUserId;
        var adminName = room.Participant1UserId == adminUserId
            ? room.Participant1Name
            : room.Participant2Name;

        if (adminUserId == otherUserId || adminUserId == currentUserId)
        {
            // Find which participant is admin
            var isAdmin1 = room.Participant1Role == "admin";
            var adminId = isAdmin1 ? room.Participant1UserId : room.Participant2UserId;
            var welcomeName = isAdmin1 ? room.Participant1Name : room.Participant2Name;

            var welcomeMsg = new ChatMessage
            {
                ChatRoomId = room.Id,
                SenderUserId = adminId,
                SenderName = welcomeName,
                Content = "Dobrodošli u Helpi. Kako vam možemo pomoći?",
            };
            welcomeMsg = await _chatRepo.AddMessageAsync(welcomeMsg);

            room.LastMessageText = welcomeMsg.Content;
            room.LastMessageAt = welcomeMsg.SentAt;
            room.LastMessageSenderUserId = adminId;
            await _chatRepo.UpdateRoomAsync(room);
        }

        return MapRoomToDto(room, currentUserId);
    }

    /// <summary>
    /// Get all chat rooms for a user. Auto-creates a support room with admin
    /// if the user has no rooms yet (so chat "just works" on first open).
    /// </summary>
    public async Task<List<ChatRoomDto>> GetUserRoomsAsync(int userId)
    {
        var rooms = await _chatRepo.GetRoomsByUserAsync(userId);

        // Auto-create support room with admin if user has no rooms
        if (rooms.Count == 0)
        {
            var currentUser = await _userRepo.GetByIdAsync(userId);
            if (currentUser != null && currentUser.UserType != UserType.Admin)
            {
                var adminIds = await _userRepo.GetAdminIdsAsync();
                if (adminIds.Count > 0)
                {
                    var adminId = adminIds[0];
                    await GetOrCreateRoomAsync(userId, adminId);
                    rooms = await _chatRepo.GetRoomsByUserAsync(userId);
                }
            }
        }

        return rooms.Select(r => MapRoomToDto(r, userId)).ToList();
    }

    /// <summary>
    /// Get paginated messages for a room. Validates user is participant.
    /// </summary>
    public async Task<List<ChatMessageDto>> GetMessagesAsync(int roomId, int userId, int page = 1, int pageSize = 50)
    {
        var room = await _chatRepo.GetRoomByIdAsync(roomId)
            ?? throw new KeyNotFoundException($"ChatRoom {roomId} not found");

        ValidateParticipant(room, userId);

        var messages = await _chatRepo.GetMessagesAsync(roomId, page, pageSize);
        return _mapper.Map<List<ChatMessageDto>>(messages);
    }

    /// <summary>
    /// Send a message. Persists to DB, updates room cache, broadcasts via SignalR.
    /// </summary>
    public async Task<ChatMessageDto> SendMessageAsync(int roomId, int senderUserId, string content)
    {
        var room = await _chatRepo.GetRoomByIdAsync(roomId)
            ?? throw new KeyNotFoundException($"ChatRoom {roomId} not found");

        ValidateParticipant(room, senderUserId);

        var senderName = room.Participant1UserId == senderUserId
            ? room.Participant1Name
            : room.Participant2Name;

        var message = new ChatMessage
        {
            ChatRoomId = roomId,
            SenderUserId = senderUserId,
            SenderName = senderName,
            Content = content,
        };

        message = await _chatRepo.AddMessageAsync(message);

        // Update room denormalized fields
        room.LastMessageText = content.Length > 200 ? content[..200] : content;
        room.LastMessageAt = message.SentAt;
        room.LastMessageSenderUserId = senderUserId;
        room.UpdatedAt = DateTime.UtcNow;

        // Increment unread for the OTHER participant
        if (room.Participant1UserId == senderUserId)
            room.Participant2UnreadCount++;
        else
            room.Participant1UnreadCount++;

        await _chatRepo.UpdateRoomAsync(room);

        var dto = _mapper.Map<ChatMessageDto>(message);

        // Broadcast to the other participant via SignalR
        var recipientUserId = room.Participant1UserId == senderUserId
            ? room.Participant2UserId
            : room.Participant1UserId;

        await _signalR.SendNotificationToUserAsync(recipientUserId,
            new HNotificationDto
            {
                Title = senderName,
                Body = content.Length > 100 ? content[..100] + "…" : content,
                Type = NotificationType.General,
            });

        return dto;
    }

    /// <summary>
    /// Mark all messages in a room as read for the given user. Returns number of messages marked.
    /// </summary>
    public async Task<int> MarkAsReadAsync(int roomId, int userId)
    {
        var room = await _chatRepo.GetRoomByIdAsync(roomId)
            ?? throw new KeyNotFoundException($"ChatRoom {roomId} not found");

        ValidateParticipant(room, userId);

        var count = await _chatRepo.MarkMessagesAsReadAsync(roomId, userId);

        // Reset unread count for this participant
        if (room.Participant1UserId == userId)
            room.Participant1UnreadCount = 0;
        else
            room.Participant2UnreadCount = 0;

        await _chatRepo.UpdateRoomAsync(room);
        return count;
    }

    /// <summary>
    /// Get total unread message count across all rooms for a user.
    /// </summary>
    public async Task<int> GetUnreadCountAsync(int userId)
        => await _chatRepo.GetUnreadCountAsync(userId);

    // ── Helpers ─────────────────────────────────────

    private static void ValidateParticipant(ChatRoom room, int userId)
    {
        if (room.Participant1UserId != userId && room.Participant2UserId != userId)
            throw new UnauthorizedAccessException("User is not a participant of this chat room");
    }

    private ChatRoomDto MapRoomToDto(ChatRoom room, int currentUserId)
    {
        var dto = _mapper.Map<ChatRoomDto>(room);
        // Set unread count for the requesting user
        dto.UnreadCount = room.Participant1UserId == currentUserId
            ? room.Participant1UnreadCount
            : room.Participant2UnreadCount;
        return dto;
    }

    private async Task<string> GetUserDisplayNameAsync(User user)
    {
        // Admin always shows as "Helpi" (brand name, not personal name)
        if (user.UserType == UserType.Admin)
            return "Helpi";

        // Try to get full name from contact info based on role
        if (user.Student?.Contact != null)
            return user.Student.Contact.FullName;
        if (user.Customer?.Contact != null)
            return user.Customer.Contact.FullName;

        // Fallback: reload with includes to get contact info
        var loaded = await _userRepo.GetByIdWithContactAsync(user.Id);
        if (loaded?.Student?.Contact != null)
            return loaded.Student.Contact.FullName;
        if (loaded?.Customer?.Contact != null)
            return loaded.Customer.Contact.FullName;

        return loaded?.UserName ?? $"User {user.Id}";
    }
}
