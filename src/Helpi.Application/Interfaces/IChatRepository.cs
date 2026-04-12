using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IChatRepository
{
    // ChatRoom
    Task<ChatRoom?> GetRoomByIdAsync(int roomId);
    Task<ChatRoom?> GetRoomByParticipantsAsync(int userId1, int userId2);
    Task<List<ChatRoom>> GetRoomsByUserAsync(int userId);
    Task<ChatRoom> CreateRoomAsync(ChatRoom room);
    Task UpdateRoomAsync(ChatRoom room);

    // ChatMessage
    Task<List<ChatMessage>> GetMessagesAsync(int roomId, int page, int pageSize);
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<int> MarkMessagesAsReadAsync(int roomId, int readerUserId);
    Task<int> GetUnreadCountAsync(int userId);
}
