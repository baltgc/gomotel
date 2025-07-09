using Gomotel.Infrastructure.MercadoPago.Models;

namespace Gomotel.Infrastructure.MercadoPago;

/// <summary>
/// Interface for MercadoPago payment processing service
/// </summary>
public interface IMercadoPagoPaymentService
{
    /// <summary>
    /// Create a payment in MercadoPago
    /// </summary>
    /// <param name="request">Payment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment response</returns>
    Task<MercadoPagoPaymentResponse> CreatePaymentAsync(
        MercadoPagoPaymentRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get payment status from MercadoPago
    /// </summary>
    /// <param name="paymentId">MercadoPago payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment response</returns>
    Task<MercadoPagoPaymentResponse> GetPaymentAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get payment by external reference
    /// </summary>
    /// <param name="externalReference">External reference (our internal payment ID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment response</returns>
    Task<MercadoPagoPaymentResponse?> GetPaymentByExternalReferenceAsync(
        string externalReference,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Cancel a payment in MercadoPago
    /// </summary>
    /// <param name="paymentId">MercadoPago payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment response</returns>
    Task<MercadoPagoPaymentResponse> CancelPaymentAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Refund a payment in MercadoPago
    /// </summary>
    /// <param name="paymentId">MercadoPago payment ID</param>
    /// <param name="amount">Amount to refund (null for full refund)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refund response</returns>
    Task<MercadoPagoRefundResponse> RefundPaymentAsync(
        long paymentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// MercadoPago refund response
/// </summary>
public class MercadoPagoRefundResponse
{
    /// <summary>
    /// Refund ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Payment ID
    /// </summary>
    public long PaymentId { get; set; }

    /// <summary>
    /// Refund amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Refund status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Date when refund was created
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Whether refund was successful
    /// </summary>
    public bool IsSuccessful => Status == "approved";
}
