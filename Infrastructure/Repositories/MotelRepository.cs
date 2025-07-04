using Gomotel.Domain.Entities;
using Gomotel.Domain.Repositories;
using Gomotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gomotel.Infrastructure.Repositories;

public class MotelRepository : IMotelRepository
{
    private readonly ApplicationDbContext _context;

    public MotelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Motel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context
            .Motels.Include(m => m.Rooms)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Motel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context
            .Motels.Where(m => m.IsActive)
            .Include(m => m.Rooms)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Motel>> GetByOwnerIdAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Motels.Where(m => m.OwnerId == ownerId)
            .Include(m => m.Rooms)
            .ToListAsync(cancellationToken);
    }

    public async Task<Motel> AddAsync(Motel motel, CancellationToken cancellationToken = default)
    {
        _context.Motels.Add(motel);
        await _context.SaveChangesAsync(cancellationToken);
        return motel;
    }

    public async Task UpdateAsync(Motel motel, CancellationToken cancellationToken = default)
    {
        // For in-memory database, we need to handle updates carefully
        var existingMotel = await _context
            .Motels.Include(m => m.Rooms)
            .FirstOrDefaultAsync(m => m.Id == motel.Id, cancellationToken);

        if (existingMotel != null)
        {
            // Update the existing entity's properties
            existingMotel.UpdateDetails(
                motel.Name,
                motel.Description,
                motel.PhoneNumber,
                motel.Email
            );

            // Handle new rooms
            foreach (var room in motel.Rooms)
            {
                if (!existingMotel.Rooms.Any(r => r.Id == room.Id))
                {
                    // Add the room directly to the context
                    _context.Rooms.Add(room);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new InvalidOperationException($"Motel with ID {motel.Id} not found");
        }
    }

    public async Task DeleteAsync(Motel motel, CancellationToken cancellationToken = default)
    {
        _context.Motels.Remove(motel);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Motels.AnyAsync(m => m.Id == id, cancellationToken);
    }
}
