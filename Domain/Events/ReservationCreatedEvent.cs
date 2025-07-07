namespace Gomotel.Domain.Events;

public record ReservationCreatedEvent(Guid ReservationId, Guid UserId, Guid MotelId, Guid RoomId)
    : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
