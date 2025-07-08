using Gomotel.Domain.Entities;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Repositories;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Room>> GetByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Room>> GetAvailableRoomsByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Room>> GetAvailableRoomsForTimeRangeAsync(
        Guid motelId,
        TimeRange timeRange,
        int? capacity = null,
        CancellationToken cancellationToken = default
    );
    Task<Room?> GetByRoomNumberAsync(
        Guid motelId,
        string roomNumber,
        CancellationToken cancellationToken = default
    );
    Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default);
    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);
    Task DeleteAsync(Room room, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsRoomNumberTakenAsync(
        Guid motelId,
        string roomNumber,
        CancellationToken cancellationToken = default
    );
}
