using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.Services;
using Gomotel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gomotel.Controllers;

[ApiController]
[Route("api/motels/{motelId}/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IMotelService _motelService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IMotelService motelService, ILogger<RoomsController> logger)
    {
        _motelService = motelService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms(Guid motelId)
    {
        try
        {
            var rooms = await _motelService.GetRoomsAsync(motelId, availableOnly: true);
            return Ok(rooms);
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rooms for motel {MotelId}", motelId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{roomId}")]
    [AllowAnonymous]
    public async Task<ActionResult<Room>> GetRoom(Guid motelId, Guid roomId)
    {
        try
        {
            var room = await _motelService.GetRoomByIdAsync(motelId, roomId);
            return Ok(room);
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (RoomNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving room {RoomId} for motel {MotelId}",
                roomId,
                motelId
            );
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<ActionResult<Room>> CreateRoom(
        Guid motelId,
        [FromBody] CreateRoomRequest request
    )
    {
        try
        {
            // TODO: Add authorization check - MotelAdmin can only manage their own motels
            // if (User.IsInRole("MotelAdmin") && motel.OwnerId != GetCurrentUserId())
            //     return Forbid();

            var pricePerHour = Money.Create(request.PricePerHour, request.Currency ?? "USD");

            var room = await _motelService.CreateRoomAsync(
                motelId,
                request.RoomNumber,
                request.Name,
                request.Description,
                request.Type,
                request.Capacity,
                pricePerHour,
                request.ImageUrl
            );

            return CreatedAtAction(
                nameof(GetRoom),
                new { motelId = motelId, roomId = room.Id },
                room
            );
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (DuplicateRoomNumberException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room for motel {MotelId}", motelId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{roomId}")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<IActionResult> UpdateRoom(
        Guid motelId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request
    )
    {
        try
        {
            // TODO: Add authorization check

            var pricePerHour = Money.Create(request.PricePerHour, request.Currency ?? "USD");

            await _motelService.UpdateRoomAsync(
                motelId,
                roomId,
                request.Name,
                request.Description,
                request.Capacity,
                pricePerHour
            );

            return NoContent();
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (RoomNotFoundException ex)
        {
            return NotFound(ex.Message);
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
            _logger.LogError(
                ex,
                "Error updating room {RoomId} for motel {MotelId}",
                roomId,
                motelId
            );
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPatch("{roomId}/availability")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<IActionResult> UpdateRoomAvailability(
        Guid motelId,
        Guid roomId,
        [FromBody] UpdateAvailabilityRequest request
    )
    {
        try
        {
            await _motelService.UpdateRoomAvailabilityAsync(motelId, roomId, request.IsAvailable);
            return NoContent();
        }
        catch (MotelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (RoomNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for room {RoomId}", roomId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{roomId}/availability")]
    [AllowAnonymous]
    public async Task<ActionResult<RoomAvailabilityResponse>> CheckRoomAvailability(
        Guid motelId,
        Guid roomId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime
    )
    {
        try
        {
            var room = await _motelService.GetRoomByIdAsync(motelId, roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found in motel {motelId}");
            }

            var timeRange = TimeRange.Create(startTime, endTime);
            var isAvailable = _motelService.IsRoomAvailableForTimeRange(room, timeRange);

            return Ok(
                new RoomAvailabilityResponse
                {
                    RoomId = roomId,
                    IsAvailable = isAvailable,
                    StartTime = startTime,
                    EndTime = endTime,
                    PricePerHour = room.PricePerHour.Amount,
                    Currency = room.PricePerHour.Currency,
                    TotalPrice = room.PricePerHour.Amount * (decimal)timeRange.Duration.TotalHours,
                }
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
            _logger.LogError(ex, "Error checking availability for room {RoomId}", roomId);
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs
public record CreateRoomRequest(
    string RoomNumber,
    string Name,
    string Description,
    RoomType Type,
    int Capacity,
    decimal PricePerHour,
    string? Currency = "USD",
    string? ImageUrl = null
);

public record UpdateRoomRequest(
    string Name,
    string Description,
    int Capacity,
    decimal PricePerHour,
    string? Currency = "USD"
);

public record UpdateAvailabilityRequest(bool IsAvailable);

public record RoomAvailabilityResponse
{
    public Guid RoomId { get; init; }
    public bool IsAvailable { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public decimal PricePerHour { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal TotalPrice { get; init; }
}
