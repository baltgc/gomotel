using Gomotel.Domain.Entities;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Reservation>> GetByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Reservation>> GetByRoomIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default
    );
    Task<bool> HasOverlappingReservationAsync(
        Guid roomId,
        TimeRange timeRange,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default
    );
    Task<Reservation> AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
