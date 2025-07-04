using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gomotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotelsController : ControllerBase
{
    private readonly IMotelRepository _motelRepository;
    private readonly ILogger<MotelsController> _logger;

    public MotelsController(IMotelRepository motelRepository, ILogger<MotelsController> logger)
    {
        _motelRepository = motelRepository;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Motel>>> GetMotels()
    {
        try
        {
            var motels = await _motelRepository.GetAllAsync();
            return Ok(motels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving motels");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Motel>> GetMotel(Guid id)
    {
        try
        {
            var motel = await _motelRepository.GetByIdAsync(id);
            if (motel == null)
            {
                return NotFound();
            }
            return Ok(motel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving motel {MotelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<ActionResult<Motel>> CreateMotel([FromBody] CreateMotelRequest request)
    {
        try
        {
            var address = Address.Create(
                request.Street,
                request.City,
                request.State,
                request.ZipCode,
                request.Country
            );

            var motel = Motel.Create(
                request.Name,
                request.Description,
                address,
                request.PhoneNumber,
                request.Email,
                request.OwnerId,
                request.ImageUrl
            );

            var createdMotel = await _motelRepository.AddAsync(motel);
            return CreatedAtAction(nameof(GetMotel), new { id = createdMotel.Id }, createdMotel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating motel");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,MotelAdmin")]
    public async Task<IActionResult> UpdateMotel(Guid id, [FromBody] UpdateMotelRequest request)
    {
        try
        {
            var motel = await _motelRepository.GetByIdAsync(id);
            if (motel == null)
            {
                return NotFound();
            }

            motel.UpdateDetails(
                request.Name,
                request.Description,
                request.PhoneNumber,
                request.Email
            );

            if (request.Address != null)
            {
                var address = Address.Create(
                    request.Address.Street,
                    request.Address.City,
                    request.Address.State,
                    request.Address.ZipCode,
                    request.Address.Country
                );
                motel.UpdateAddress(address);
            }

            await _motelRepository.UpdateAsync(motel);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating motel {MotelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMotel(Guid id)
    {
        try
        {
            var motel = await _motelRepository.GetByIdAsync(id);
            if (motel == null)
            {
                return NotFound();
            }

            await _motelRepository.DeleteAsync(motel);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting motel {MotelId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

public record CreateMotelRequest(
    string Name,
    string Description,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    string PhoneNumber,
    string Email,
    Guid OwnerId,
    string? ImageUrl = null
);

public record UpdateMotelRequest(
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    AddressRequest? Address = null
);

public record AddressRequest(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country
);
