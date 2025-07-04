using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gomotel.Controllers;

[ApiController]
[Route("api/motels/{motelId}/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IMotelRepository _motelRepository;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IMotelRepository motelRepository, ILogger<RoomsController> logger)
    {
        _motelRepository = motelRepository;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms(Guid motelId)
    {
        try
        {
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            return Ok(motel.Rooms.Where(r => r.IsAvailable));
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
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found in motel {motelId}");
            }

            return Ok(room);
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
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            // TODO: Add authorization check - MotelAdmin can only manage their own motels
            // if (User.IsInRole("MotelAdmin") && motel.OwnerId != GetCurrentUserId())
            //     return Forbid();

            var pricePerHour = Money.Create(request.PricePerHour, request.Currency ?? "USD");

            var room = Room.Create(
                motelId,
                request.RoomNumber,
                request.Name,
                request.Description,
                request.Type,
                request.Capacity,
                pricePerHour,
                request.ImageUrl
            );

            motel.AddRoom(room);
            await _motelRepository.UpdateAsync(motel);

            return CreatedAtAction(
                nameof(GetRoom),
                new { motelId = motelId, roomId = room.Id },
                room
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
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
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found in motel {motelId}");
            }

            // TODO: Add authorization check

            var pricePerHour = Money.Create(request.PricePerHour, request.Currency ?? "USD");
            room.UpdateDetails(request.Name, request.Description, request.Capacity, pricePerHour);

            await _motelRepository.UpdateAsync(motel);
            return NoContent();
        }
        catch (ArgumentException ex)
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
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found in motel {motelId}");
            }

            room.SetAvailability(request.IsAvailable);
            await _motelRepository.UpdateAsync(motel);

            return NoContent();
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
            var motel = await _motelRepository.GetByIdAsync(motelId);
            if (motel == null)
            {
                return NotFound($"Motel with ID {motelId} not found");
            }

            var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found in motel {motelId}");
            }

            var timeRange = TimeRange.Create(startTime, endTime);
            var isAvailable = room.IsAvailableForTimeRange(timeRange);

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
        catch (ArgumentException ex)
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
