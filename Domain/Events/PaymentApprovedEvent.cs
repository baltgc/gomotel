namespace Gomotel.Domain.Events;

public record PaymentApprovedEvent(Guid PaymentId, Guid ReservationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
