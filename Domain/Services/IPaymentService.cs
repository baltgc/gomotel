using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

/// <summary>
/// Service interface for payment operations using MercadoPago
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Get a payment by ID
    /// </summary>
    Task<Payment?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a payment by reservation ID
    /// </summary>
    Task<Payment?> GetPaymentByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get all payments for a user
    /// </summary>
    Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get payments by status
    /// </summary>
    Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get payments within a date range
    /// </summary>
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create a payment for a reservation
    /// </summary>
    Task<Payment> CreatePaymentForReservationAsync(
        Guid reservationId,
        string paymentMethod,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Process a payment through MercadoPago
    /// </summary>
    Task<Payment> ProcessPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Approve a payment (used by webhooks)
    /// </summary>
    Task<Payment> ApprovePaymentAsync(
        Guid paymentId,
        string transactionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fail a payment (used by webhooks)
    /// </summary>
    Task<Payment> FailPaymentAsync(
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Refund a payment through MercadoPago
    /// </summary>
    Task<Payment> RefundPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a reservation has a pending payment
    /// </summary>
    Task<bool> HasPendingPaymentForReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    );
}
