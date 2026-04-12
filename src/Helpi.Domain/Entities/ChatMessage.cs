using System.ComponentModel.DataAnnotations.Schema;

namespace Helpi.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }

    [ForeignKey(nameof(ChatRoom))]
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;

    public int SenderUserId { get; set; }
    public string SenderName { get; set; } = null!;

    [Column(TypeName = "text")]
    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; }
}
