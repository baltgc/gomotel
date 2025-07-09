using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.Services;
using Gomotel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gomotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> GetPayment(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound($"Payment with ID {id} not found");
            }

            // TODO: Add authorization check
            // Users can only see their own payments
            // MotelAdmins can see payments for their motels
            // Admins can see all payments

            return Ok(MapToDto(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get payment for a specific reservation
    /// </summary>
    [HttpGet("reservation/{reservationId}")]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> GetPaymentByReservation(Guid reservationId)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByReservationIdAsync(reservationId);
            if (payment == null)
            {
                return NotFound($"No payment found for reservation {reservationId}");
            }

            // TODO: Add authorization check

            return Ok(MapToDto(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving payment for reservation {ReservationId}",
                reservationId
            );
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all payments for a user
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(Guid userId)
    {
        try
        {
            // TODO: Add authorization check - Users can only see their own payments

            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get payments by status (Admin only)
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByStatus(
        PaymentStatus status
    )
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByStatusAsync(status);
            return Ok(payments.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments with status {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get payments within a date range (Admin only)
    /// </summary>
    [HttpGet("date-range")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate
    )
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var payments = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);
            return Ok(payments.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving payments between {StartDate} and {EndDate}",
                startDate,
                endDate
            );
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create and process a payment for a reservation through MercadoPago
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<PaymentDto>> CreateAndProcessPayment(
        [FromBody] CreatePaymentRequest request
    )
    {
        try
        {
            // TODO: Add authorization check - Users can only create payments for their own reservations

            // Create payment
            var payment = await _paymentService.CreatePaymentForReservationAsync(
                request.ReservationId,
                request.PaymentMethod
            );

            // Process payment through MercadoPago
            var processedPayment = await _paymentService.ProcessPaymentAsync(payment.Id);

            return CreatedAtAction(
                nameof(GetPayment),
                new { id = processedPayment.Id },
                MapToDto(processedPayment)
            );
        }
        catch (ReservationNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating and processing payment for reservation {ReservationId}",
                request.ReservationId
            );
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Refund a payment through MercadoPago (Admin/MotelAdmin only)
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<ActionResult<PaymentDto>> RefundPayment(Guid id)
    {
        try
        {
            // TODO: Add authorization check - MotelAdmins can only refund payments for their motels

            var payment = await _paymentService.RefundPaymentAsync(id);
            return Ok(MapToDto(payment));
        }
        catch (PaymentNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            ReservationId = payment.ReservationId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = payment.Status,
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            FailureReason = payment.FailureReason,
            ProcessedAt = payment.ProcessedAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt,
        };
    }
}

/// <summary>
/// Request model for creating a payment
/// </summary>
public record CreatePaymentRequest(Guid ReservationId, string PaymentMethod);

/// <summary>
/// Payment data transfer object
/// </summary>
public record PaymentDto
{
    public Guid Id { get; init; }
    public Guid ReservationId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public PaymentStatus Status { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? TransactionId { get; init; }
    public string? FailureReason { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
