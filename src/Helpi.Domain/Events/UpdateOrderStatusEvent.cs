
namespace Helpi.Domain.Events;

public record UpdateOrderStatusEvent(int OrderId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

}
