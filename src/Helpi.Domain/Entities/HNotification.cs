
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities;
public class HNotification
{
    public int Id { get; set; }
    public int RecieverUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public string? Payload { get; set; } // JSON data
}