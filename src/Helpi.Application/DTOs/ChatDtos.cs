using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;

// ── Response DTOs ──────────────────────────────────

public class ChatRoomDto
{
    public int Id { get; set; }
    public int Participant1UserId { get; set; }
    public string Participant1Name { get; set; } = null!;
    public string Participant1Role { get; set; } = null!;
    public int Participant2UserId { get; set; }
    public string Participant2Name { get; set; } = null!;
    public string Participant2Role { get; set; } = null!;
    public string? LastMessageText { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int? LastMessageSenderUserId { get; set; }
    public int UnreadCount { get; set; } // computed per-user
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; }
}

// ── Create / Send DTOs ─────────────────────────────

public class CreateChatRoomDto
{
    [Required]
    public int OtherUserId { get; set; }
}

public class SendMessageDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = null!;
}
