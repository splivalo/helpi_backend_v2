
namespace Helpi.Domain.Events;

public record ReinitiateAllFailedMatchesEvent() : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
