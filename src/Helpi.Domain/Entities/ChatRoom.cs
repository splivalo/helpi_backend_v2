using System.ComponentModel.DataAnnotations.Schema;

namespace Helpi.Domain.Entities;

public class ChatRoom
{
    public int Id { get; set; }

    // Participant 1 (initiator)
    public int Participant1UserId { get; set; }
    public string Participant1Name { get; set; } = null!;
    public string Participant1Role { get; set; } = null!; // "admin", "student", "senior"

    // Participant 2
    public int Participant2UserId { get; set; }
    public string Participant2Name { get; set; } = null!;
    public string Participant2Role { get; set; } = null!;

    // Last message cache (denormalized for list performance)
    [Column(TypeName = "text")]
    public string? LastMessageText { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int? LastMessageSenderUserId { get; set; }

    // Unread counts
    public int Participant1UnreadCount { get; set; }
    public int Participant2UnreadCount { get; set; }

    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<ChatMessage> Messages { get; set; } = new();
}
