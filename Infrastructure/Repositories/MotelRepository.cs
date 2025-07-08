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
            .ThenInclude(r => r.Reservations)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Motel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context
            .Motels.Where(m => m.IsActive)
            .Include(m => m.Rooms)
            .ThenInclude(r => r.Reservations)
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
            .ThenInclude(r => r.Reservations)
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
        _context.Motels.Update(motel);
        await _context.SaveChangesAsync(cancellationToken);
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
