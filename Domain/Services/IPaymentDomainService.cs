using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

/// <summary>
/// Domain service for payment entity operations
/// </summary>
public interface IPaymentDomainService
{
    /// <summary>
    /// Create a new payment
    /// </summary>
    Payment CreatePayment(Guid reservationId, Money amount, string paymentMethod);

    /// <summary>
    /// Process a payment (move to Processing status)
    /// </summary>
    void ProcessPayment(Payment payment);

    /// <summary>
    /// Approve a payment (mark as successful)
    /// </summary>
    void ApprovePayment(Payment payment, string transactionId);

    /// <summary>
    /// Fail a payment (mark as failed)
    /// </summary>
    void FailPayment(Payment payment, string reason);

    /// <summary>
    /// Refund a payment (mark as refunded)
    /// </summary>
    void RefundPayment(Payment payment);

    /// <summary>
    /// Check if payment is successful
    /// </summary>
    bool IsPaymentSuccessful(Payment payment);

    /// <summary>
    /// Check if payment is failed
    /// </summary>
    bool IsPaymentFailed(Payment payment);

    /// <summary>
    /// Check if payment is refunded
    /// </summary>
    bool IsPaymentRefunded(Payment payment);
}
