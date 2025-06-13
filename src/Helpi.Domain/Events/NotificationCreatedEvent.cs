using Helpi.Domain.Enums;

namespace Helpi.Domain.Events;

public class NotificationCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int NotificationId { get; }
    public int UserId { get; }
    public string Title { get; }
    public string Body { get; }
    public NotificationType Type { get; }
    public string? Payload { get; }

    public NotificationCreatedEvent(int notificationId, int userId, string title, string body, NotificationType type, string? payload = null)
    {
        NotificationId = notificationId;
        UserId = userId;
        Title = title;
        Body = body;
        Type = type;
        Payload = payload;
    }
}