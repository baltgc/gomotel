using Gomotel.Domain.Entities;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;
using Gomotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gomotel.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reservations.Include(r => r.Motel)
            .Include(r => r.Room)
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reservations.Where(r => r.UserId == userId)
            .Include(r => r.Motel)
            .Include(r => r.Room)
            .Include(r => r.Payment)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reservations.Where(r => r.MotelId == motelId)
            .Include(r => r.Room)
            .Include(r => r.Payment)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByRoomIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Reservations.Where(r => r.RoomId == roomId)
            .Include(r => r.Payment)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingReservationAsync(
        Guid roomId,
        TimeRange timeRange,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context
            .Reservations.Where(r => r.RoomId == roomId)
            .Where(r =>
                r.Status == Domain.Enums.ReservationStatus.Confirmed
                || r.Status == Domain.Enums.ReservationStatus.CheckedIn
            );

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync(
            r =>
                r.TimeRange.StartTime < timeRange.EndTime
                && r.TimeRange.EndTime > timeRange.StartTime,
            cancellationToken
        );
    }

    public async Task<Reservation> AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    )
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task UpdateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    )
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    )
    {
        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
