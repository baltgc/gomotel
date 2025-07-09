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
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly IMotelService _motelService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationService reservationService,
        IMotelService motelService,
        IPaymentService paymentService,
        ILogger<ReservationsController> logger
    )
    {
        _reservationService = reservationService;
        _motelService = motelService;
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? motelId = null
    )
    {
        try
        {
            IEnumerable<Reservation> reservations;

            if (userId.HasValue)
            {
                // TODO: Add authorization check - Users can only see their own reservations
                reservations = await _reservationService.GetReservationsByUserIdAsync(userId.Value);
            }
            else if (motelId.HasValue)
            {
                // TODO: Add authorization check - MotelAdmins can only see their motel's reservations
                reservations = await _reservationService.GetReservationsByMotelIdAsync(
                    motelId.Value
                );
            }
            else
            {
                // Only Admins can see all reservations
                if (!User.IsInRole("Admin"))
                {
                    return Forbid("Only administrators can view all reservations");
                }
                // For now, return empty - we'd need a GetAllAsync method
                reservations = new List<Reservation>();
            }

            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Reservation>> GetReservation(Guid id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                throw new ReservationNotFoundException(id);
            }

            // TODO: Add authorization check
            // Users can only see their own reservations
            // MotelAdmins can see reservations for their motels
            // Admins can see all reservations

            return Ok(reservation);
        }
        catch (ReservationNotFoundException)
        {
            return NotFound($"Reservation with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<Reservation>> CreateReservation(
        [FromBody] CreateReservationRequest request
    )
    {
        try
        {
            // TODO: Get current user ID from JWT token
            var userId = request.UserId; // This should come from the authenticated user

            var createdReservation = await _reservationService.CreateReservationAsync(
                request.MotelId,
                request.RoomId,
                userId,
                request.StartTime,
                request.EndTime,
                request.SpecialRequests
            );

            return CreatedAtAction(
                nameof(GetReservation),
                new { id = createdReservation.Id },
                createdReservation
            );
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (RoomNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (RoomUnavailableException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (BookingConflictException ex)
        {
            return Conflict(ex.Message);
        }
        catch (InvalidBookingTimeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/confirm")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<IActionResult> ConfirmReservation(Guid id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                throw new ReservationNotFoundException(id);
            }

            // TODO: Add authorization check - MotelAdmins can only confirm reservations for their motels

            _reservationService.ConfirmReservation(reservation);
            await _reservationService.UpdateReservationAsync(reservation);

            return NoContent();
        }
        catch (ReservationNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidReservationOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/checkin")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<IActionResult> CheckInReservation(Guid id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                throw new ReservationNotFoundException(id);
            }

            _reservationService.CheckInReservation(reservation);
            await _reservationService.UpdateReservationAsync(reservation);

            return NoContent();
        }
        catch (ReservationNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidReservationOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/checkout")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<IActionResult> CheckOutReservation(Guid id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                throw new ReservationNotFoundException(id);
            }

            _reservationService.CheckOutReservation(reservation);
            await _reservationService.UpdateReservationAsync(reservation);

            return NoContent();
        }
        catch (ReservationNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidReservationOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                throw new ReservationNotFoundException(id);
            }

            // TODO: Add authorization check - Users can cancel their own reservations, MotelAdmins can cancel reservations for their motels

            _reservationService.CancelReservation(reservation);
            await _reservationService.UpdateReservationAsync(reservation);

            return NoContent();
        }
        catch (ReservationNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ReservationCancellationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidReservationOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create and process payment for a reservation through MercadoPago
    /// </summary>
    [HttpPost("{id}/pay")]
    [Authorize]
    public async Task<ActionResult<PaymentDto>> PayForReservation(
        Guid id,
        [FromBody] CreatePaymentForReservationRequest request
    )
    {
        try
        {
            // TODO: Add authorization check - Users can only pay for their own reservations

            // Create payment
            var payment = await _paymentService.CreatePaymentForReservationAsync(
                id,
                request.PaymentMethod
            );

            // Process payment through MercadoPago
            var processedPayment = await _paymentService.ProcessPaymentAsync(payment.Id);

            return CreatedAtAction(
                "GetPayment",
                "Payments",
                new { id = processedPayment.Id },
                MapPaymentToDto(processedPayment)
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
                id
            );
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("availability")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RoomAvailabilityInfo>>> CheckAvailability(
        [FromQuery] Guid motelId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] int? capacity = null
    )
    {
        try
        {
            var timeRange = TimeRange.Create(startTime, endTime);
            var availableRooms = await _motelService.GetAvailableRoomsAsync(
                motelId,
                timeRange,
                capacity
            );

            var availabilityInfo = availableRooms.Select(room => new RoomAvailabilityInfo
            {
                RoomId = room.Id,
                RoomNumber = room.RoomNumber,
                Name = room.Name,
                Type = room.Type,
                Capacity = room.Capacity,
                PricePerHour = room.PricePerHour.Amount,
                Currency = room.PricePerHour.Currency,
                TotalPrice = room.PricePerHour.Amount * (decimal)timeRange.Duration.TotalHours,
                ImageUrl = room.ImageUrl,
            });

            return Ok(availabilityInfo);
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidBookingTimeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for motel {MotelId}", motelId);
            return StatusCode(500, "Internal server error");
        }
    }

    private static PaymentDto MapPaymentToDto(Payment payment)
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

// DTOs
public record CreateReservationRequest(
    Guid MotelId,
    Guid RoomId,
    Guid UserId, // TODO: This should come from the authenticated user instead
    DateTime StartTime,
    DateTime EndTime,
    string? SpecialRequests = null
);

public record CreatePaymentForReservationRequest(string PaymentMethod);

public record RoomAvailabilityInfo
{
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public RoomType Type { get; init; }
    public int Capacity { get; init; }
    public decimal PricePerHour { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal TotalPrice { get; init; }
    public string? ImageUrl { get; init; }
}
