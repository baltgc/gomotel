using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gomotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMotelRepository _motelRepository;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationRepository reservationRepository,
        IMotelRepository motelRepository,
        ILogger<ReservationsController> logger
    )
    {
        _reservationRepository = reservationRepository;
        _motelRepository = motelRepository;
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
                reservations = await _reservationRepository.GetByUserIdAsync(userId.Value);
            }
            else if (motelId.HasValue)
            {
                // TODO: Add authorization check - MotelAdmins can only see their motel's reservations
                reservations = await _reservationRepository.GetByMotelIdAsync(motelId.Value);
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
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found");
            }

            // TODO: Add authorization check
            // Users can only see their own reservations
            // MotelAdmins can see reservations for their motels
            // Admins can see all reservations

            return Ok(reservation);
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
            // Validate the motel and room exist
            var motel = await _motelRepository.GetByIdAsync(request.MotelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {request.MotelId} not found");
            }

            var room = motel.Rooms.FirstOrDefault(r => r.Id == request.RoomId);
            if (room == null)
            {
                return NotFound(
                    $"Room with ID {request.RoomId} not found in motel {request.MotelId}"
                );
            }

            // Create time range and validate
            var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

            // Check for overlapping reservations
            var hasOverlapping = await _reservationRepository.HasOverlappingReservationAsync(
                request.RoomId,
                timeRange
            );

            if (hasOverlapping)
            {
                return Conflict("The selected time slot is not available for this room");
            }

            // Calculate total amount
            var duration = timeRange.Duration;
            var totalAmount = Money.Create(
                room.PricePerHour.Amount * (decimal)duration.TotalHours,
                room.PricePerHour.Currency
            );

            // TODO: Get current user ID from JWT token
            var userId = request.UserId; // This should come from the authenticated user

            var reservation = Reservation.Create(
                request.MotelId,
                request.RoomId,
                userId,
                timeRange,
                totalAmount,
                request.SpecialRequests
            );

            var createdReservation = await _reservationRepository.AddAsync(reservation);

            return CreatedAtAction(
                nameof(GetReservation),
                new { id = createdReservation.Id },
                createdReservation
            );
        }
        catch (ArgumentException ex)
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
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found");
            }

            // TODO: Add authorization check - MotelAdmins can only confirm reservations for their motels

            reservation.Confirm();
            await _reservationRepository.UpdateAsync(reservation);

            return NoContent();
        }
        catch (InvalidOperationException ex)
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
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found");
            }

            reservation.CheckIn();
            await _reservationRepository.UpdateAsync(reservation);

            return NoContent();
        }
        catch (InvalidOperationException ex)
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
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found");
            }

            reservation.CheckOut();
            await _reservationRepository.UpdateAsync(reservation);

            return NoContent();
        }
        catch (InvalidOperationException ex)
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
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found");
            }

            // TODO: Add authorization check - Users can cancel their own reservations, MotelAdmins can cancel reservations for their motels

            reservation.Cancel();
            await _reservationRepository.UpdateAsync(reservation);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
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
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            var timeRange = TimeRange.Create(startTime, endTime);
            var availableRooms = new List<RoomAvailabilityInfo>();

            foreach (var room in motel.Rooms.Where(r => r.IsAvailable))
            {
                if (capacity.HasValue && room.Capacity < capacity.Value)
                    continue;

                var isAvailable = room.IsAvailableForTimeRange(timeRange);
                if (isAvailable)
                {
                    availableRooms.Add(
                        new RoomAvailabilityInfo
                        {
                            RoomId = room.Id,
                            RoomNumber = room.RoomNumber,
                            Name = room.Name,
                            Type = room.Type,
                            Capacity = room.Capacity,
                            PricePerHour = room.PricePerHour.Amount,
                            Currency = room.PricePerHour.Currency,
                            TotalPrice =
                                room.PricePerHour.Amount * (decimal)timeRange.Duration.TotalHours,
                            ImageUrl = room.ImageUrl,
                        }
                    );
                }
            }

            return Ok(availableRooms);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for motel {MotelId}", motelId);
            return StatusCode(500, "Internal server error");
        }
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
