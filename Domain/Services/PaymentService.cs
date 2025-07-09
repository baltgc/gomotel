using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.Repositories;
using Gomotel.Infrastructure.MercadoPago;
using Gomotel.Infrastructure.MercadoPago.Models;
using Microsoft.Extensions.Logging;

namespace Gomotel.Domain.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly PaymentDomainService _paymentDomainService;
    private readonly ReservationDomainService _reservationDomainService;
    private readonly IMercadoPagoPaymentService _mercadoPagoService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IReservationRepository reservationRepository,
        PaymentDomainService paymentDomainService,
        ReservationDomainService reservationDomainService,
        IMercadoPagoPaymentService mercadoPagoService,
        ILogger<PaymentService> logger
    )
    {
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _paymentDomainService = paymentDomainService;
        _reservationDomainService = reservationDomainService;
        _mercadoPagoService = mercadoPagoService;
        _logger = logger;
    }

    public async Task<Payment?> GetPaymentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _paymentRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Payment?> GetPaymentByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _paymentRepository.GetByReservationIdAsync(reservationId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _paymentRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default
    )
    {
        return await _paymentRepository.GetByStatusAsync(status, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        return await _paymentRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
    }

    public async Task<Payment> CreatePaymentForReservationAsync(
        Guid reservationId,
        string paymentMethod,
        CancellationToken cancellationToken = default
    )
    {
        // Get the reservation to validate and get amount
        var reservation = await _reservationRepository.GetByIdAsync(
            reservationId,
            cancellationToken
        );
        if (reservation == null)
        {
            throw new ReservationNotFoundException(reservationId);
        }

        // Check if reservation already has a payment
        var existingPayment = await _paymentRepository.GetByReservationIdAsync(
            reservationId,
            cancellationToken
        );
        if (existingPayment != null)
        {
            throw new BusinessRuleViolationException(
                "PaymentCreation",
                $"Reservation {reservationId} already has a payment"
            );
        }

        // Create payment using domain service
        var payment = _paymentDomainService.CreatePayment(
            reservationId,
            reservation.TotalAmount,
            paymentMethod
        );

        // Persist the payment
        var createdPayment = await _paymentRepository.AddAsync(payment, cancellationToken);

        // Assign payment to reservation
        _reservationDomainService.AssignPayment(reservation, createdPayment.Id);
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);

        return createdPayment;
    }

    /// <summary>
    /// Process payment through MercadoPago
    /// </summary>
    public async Task<Payment> ProcessPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new PaymentNotFoundException(paymentId);
        }

        // Get the associated reservation for payer information
        var reservation = await _reservationRepository.GetByIdAsync(
            payment.ReservationId,
            cancellationToken
        );
        if (reservation == null)
        {
            throw new ReservationNotFoundException(payment.ReservationId);
        }

        try
        {
            // Process payment using domain service first
            _paymentDomainService.ProcessPayment(payment);

            // Create MercadoPago payment request
            var mercadoPagoRequest = new MercadoPagoPaymentRequest
            {
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency,
                Description = $"Hotel Room Reservation - {reservation.Id}",
                ExternalReference = payment.Id.ToString(),
                PaymentMethodId = payment.PaymentMethod,
                Payer = new MercadoPagoPayerRequest
                {
                    Email = $"customer-{reservation.UserId}@example.com", // TODO: Get from user service
                    FirstName = "Customer", // TODO: Get from user service
                    LastName = "User", // TODO: Get from user service
                },
                Metadata = new Dictionary<string, string>
                {
                    ["reservation_id"] = reservation.Id.ToString(),
                    ["motel_id"] = reservation.MotelId.ToString(),
                    ["room_id"] = reservation.RoomId.ToString(),
                    ["user_id"] = reservation.UserId.ToString(),
                },
            };

            // Process payment through MercadoPago
            var mercadoPagoResponse = await _mercadoPagoService.CreatePaymentAsync(
                mercadoPagoRequest,
                cancellationToken
            );

            // Update payment with MercadoPago response
            if (mercadoPagoResponse.IsSuccessful)
            {
                _paymentDomainService.ApprovePayment(payment, mercadoPagoResponse.Id.ToString());
                _logger.LogInformation(
                    "MercadoPago payment approved for payment {PaymentId}",
                    paymentId
                );
            }
            else if (mercadoPagoResponse.IsFailed)
            {
                _paymentDomainService.FailPayment(payment, mercadoPagoResponse.StatusDetail);
                _logger.LogWarning(
                    "MercadoPago payment failed for payment {PaymentId}: {Reason}",
                    paymentId,
                    mercadoPagoResponse.StatusDetail
                );
            }
            else if (mercadoPagoResponse.IsPending)
            {
                // Keep payment in processing state for pending payments
                _logger.LogInformation(
                    "MercadoPago payment pending for payment {PaymentId}",
                    paymentId
                );
            }

            // Update in database
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // If payment was approved, confirm the reservation
            if (payment.Status == PaymentStatus.Approved)
            {
                _reservationDomainService.ConfirmReservation(reservation);
                await _reservationRepository.UpdateAsync(reservation, cancellationToken);
            }

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing payment {PaymentId} through MercadoPago",
                paymentId
            );

            // Mark payment as failed if MercadoPago processing fails
            _paymentDomainService.FailPayment(payment, ex.Message);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Approve a payment (used by webhooks)
    /// </summary>
    public async Task<Payment> ApprovePaymentAsync(
        Guid paymentId,
        string transactionId,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new PaymentNotFoundException(paymentId);
        }

        // Approve payment using domain service
        _paymentDomainService.ApprovePayment(payment, transactionId);

        // Update in database
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        // Confirm the associated reservation
        var reservation = await _reservationRepository.GetByIdAsync(
            payment.ReservationId,
            cancellationToken
        );
        if (reservation != null && reservation.Status == ReservationStatus.Pending)
        {
            _reservationDomainService.ConfirmReservation(reservation);
            await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        }

        return payment;
    }

    /// <summary>
    /// Fail a payment (used by webhooks)
    /// </summary>
    public async Task<Payment> FailPaymentAsync(
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new PaymentNotFoundException(paymentId);
        }

        // Fail payment using domain service
        _paymentDomainService.FailPayment(payment, reason);

        // Update in database
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return payment;
    }

    /// <summary>
    /// Refund a payment through MercadoPago
    /// </summary>
    public async Task<Payment> RefundPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new PaymentNotFoundException(paymentId);
        }

        try
        {
            // If payment has a MercadoPago transaction ID, attempt to refund through MercadoPago
            if (
                !string.IsNullOrEmpty(payment.TransactionId)
                && long.TryParse(payment.TransactionId, out var mercadoPagoId)
            )
            {
                var refundResponse = await _mercadoPagoService.RefundPaymentAsync(
                    mercadoPagoId,
                    payment.Amount.Amount,
                    cancellationToken
                );

                if (refundResponse.IsSuccessful)
                {
                    _logger.LogInformation(
                        "MercadoPago refund successful for payment {PaymentId}",
                        paymentId
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "MercadoPago refund failed for payment {PaymentId}",
                        paymentId
                    );
                }
            }

            // Refund payment using domain service
            _paymentDomainService.RefundPayment(payment);

            // Update in database
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Cancel the associated reservation if not already checked out
            var reservation = await _reservationRepository.GetByIdAsync(
                payment.ReservationId,
                cancellationToken
            );
            if (
                reservation != null
                && reservation.Status != ReservationStatus.CheckedOut
                && reservation.Status != ReservationStatus.Cancelled
            )
            {
                _reservationDomainService.CancelReservation(reservation);
                await _reservationRepository.UpdateAsync(reservation, cancellationToken);
            }

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error refunding payment {PaymentId} through MercadoPago",
                paymentId
            );

            // Still refund the payment in our system even if MercadoPago fails
            _paymentDomainService.RefundPayment(payment);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Check if a reservation has a pending payment
    /// </summary>
    public async Task<bool> HasPendingPaymentForReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _paymentRepository.HasPendingPaymentForReservationAsync(
            reservationId,
            cancellationToken
        );
    }
}
