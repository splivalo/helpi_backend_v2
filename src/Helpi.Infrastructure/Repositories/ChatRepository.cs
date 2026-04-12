using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context) => _context = context;

    // ── ChatRoom ────────────────────────────────────

    public async Task<ChatRoom?> GetRoomByIdAsync(int roomId)
        => await _context.ChatRooms.FirstOrDefaultAsync(r => r.Id == roomId);

    public async Task<ChatRoom?> GetRoomByParticipantsAsync(int userId1, int userId2)
        => await _context.ChatRooms.FirstOrDefaultAsync(r =>
            (r.Participant1UserId == userId1 && r.Participant2UserId == userId2) ||
            (r.Participant1UserId == userId2 && r.Participant2UserId == userId1));

    public async Task<List<ChatRoom>> GetRoomsByUserAsync(int userId)
        => await _context.ChatRooms
            .Where(r => (r.Participant1UserId == userId || r.Participant2UserId == userId) && !r.IsArchived)
            .OrderByDescending(r => r.LastMessageAt ?? r.CreatedAt)
            .ToListAsync();

    public async Task<ChatRoom> CreateRoomAsync(ChatRoom room)
    {
        await _context.ChatRooms.AddAsync(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task UpdateRoomAsync(ChatRoom room)
    {
        _context.ChatRooms.Update(room);
        await _context.SaveChangesAsync();
    }

    // ── ChatMessage ─────────────────────────────────

    public async Task<List<ChatMessage>> GetMessagesAsync(int roomId, int page, int pageSize)
        => await _context.ChatMessages
            .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.SentAt) // return in chronological order
            .ToListAsync();

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<int> MarkMessagesAsReadAsync(int roomId, int readerUserId)
    {
        var now = DateTime.UtcNow;
        return await _context.ChatMessages
            .Where(m => m.ChatRoomId == roomId
                     && m.SenderUserId != readerUserId
                     && m.ReadAt == null
                     && !m.IsDeleted)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ReadAt, now));
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        // Sum unread counts across all rooms where this user is a participant
        var rooms = await _context.ChatRooms
            .Where(r => (r.Participant1UserId == userId || r.Participant2UserId == userId) && !r.IsArchived)
            .ToListAsync();

        return rooms.Sum(r =>
            r.Participant1UserId == userId ? r.Participant1UnreadCount : r.Participant2UnreadCount);
    }
}
