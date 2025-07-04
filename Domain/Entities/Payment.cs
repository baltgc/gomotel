using Gomotel.Domain.Common;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Payment : AggregateRoot
{
    public Guid ReservationId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Navigation property
    public Reservation Reservation { get; private set; } = null!;

    private Payment() { } // EF Core constructor

    public static Payment Create(Guid reservationId, Money amount, string paymentMethod)
    {
        if (reservationId == Guid.Empty)
            throw new ArgumentException("Reservation ID cannot be empty", nameof(reservationId));
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method cannot be empty", nameof(paymentMethod));

        var payment = new Payment
        {
            ReservationId = reservationId,
            Amount = amount,
            Status = PaymentStatus.Created,
            PaymentMethod = paymentMethod,
        };

        return payment;
    }

    public void MarkAsProcessing()
    {
        if (Status != PaymentStatus.Created)
            throw new InvalidOperationException(
                "Only created payments can be marked as processing"
            );

        Status = PaymentStatus.Processing;
        SetUpdatedAt();
    }

    public void Approve(string transactionId)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException("Only processing payments can be approved");
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        Status = PaymentStatus.Approved;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new PaymentApprovedEvent(Id, ReservationId));
    }

    public void Fail(string reason)
    {
        if (Status is PaymentStatus.Approved or PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot fail approved or refunded payments");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Approved)
            throw new InvalidOperationException("Only approved payments can be refunded");

        Status = PaymentStatus.Refunded;
        SetUpdatedAt();
    }
}
