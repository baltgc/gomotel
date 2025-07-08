using Gomotel.Domain.Common;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Payment : AggregateRoot
{
    public Guid ReservationId { get; internal set; }
    public Money Amount { get; internal set; } = null!;
    public PaymentStatus Status { get; internal set; }
    public string PaymentMethod { get; internal set; } = string.Empty;
    public string? TransactionId { get; internal set; }
    public string? FailureReason { get; internal set; }
    public DateTime? ProcessedAt { get; internal set; }

    // Navigation property
    public Reservation Reservation { get; private set; } = null!;

    // Private constructor for EF Core
    private Payment() { }

    // Internal constructor for domain services
    internal Payment(Guid reservationId, Money amount, string paymentMethod)
    {
        ReservationId = reservationId;
        Amount = amount;
        Status = PaymentStatus.Created;
        PaymentMethod = paymentMethod;
    }

    // Internal method to update UpdatedAt timestamp
    internal void MarkAsUpdated()
    {
        SetUpdatedAt();
    }

    // Internal method to add domain events (accessible by domain services)
    internal void AddEvent(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}
