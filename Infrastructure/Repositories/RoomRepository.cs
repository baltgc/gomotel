using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;
using Gomotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gomotel.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context
            .Rooms.Include(r => r.Motel)
            .Include(r => r.Reservations)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Room>> GetByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Rooms.Where(r => r.MotelId == motelId)
            .Include(r => r.Reservations)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Rooms.Where(r => r.MotelId == motelId && r.IsAvailable)
            .Include(r => r.Reservations)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsForTimeRangeAsync(
        Guid motelId,
        TimeRange timeRange,
        int? capacity = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context.Rooms.Where(r => r.MotelId == motelId && r.IsAvailable);

        if (capacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= capacity.Value);
        }

        var rooms = await query.Include(r => r.Reservations).ToListAsync(cancellationToken);

        // Filter out rooms with overlapping reservations
        return rooms.Where(room =>
            !room.Reservations.Any(reservation =>
                reservation.Status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn
                && reservation.TimeRange.OverlapsWith(timeRange)
            )
        );
    }

    public async Task<Room?> GetByRoomNumberAsync(
        Guid motelId,
        string roomNumber,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Rooms.Include(r => r.Motel)
            .Include(r => r.Reservations)
            .FirstOrDefaultAsync(
                r => r.MotelId == motelId && r.RoomNumber == roomNumber,
                cancellationToken
            );
    }

    public async Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Room room, CancellationToken cancellationToken = default)
    {
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rooms.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> IsRoomNumberTakenAsync(
        Guid motelId,
        string roomNumber,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Rooms.AnyAsync(
            r => r.MotelId == motelId && r.RoomNumber == roomNumber,
            cancellationToken
        );
    }
}
