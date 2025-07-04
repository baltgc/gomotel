namespace Gomotel.Domain.Events;

public record ReservationCancelledEvent(Guid ReservationId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
