using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public interface IMotelService
{
    // Motel CRUD operations
    Task<IEnumerable<Motel>> GetAllMotelsAsync(CancellationToken cancellationToken = default);
    Task<Motel?> GetMotelByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Motel>> GetMotelsByOwnerIdAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default
    );
    Task<Motel> CreateMotelAsync(
        string name,
        string description,
        Address address,
        string phoneNumber,
        string email,
        Guid ownerId,
        string? imageUrl = null,
        CancellationToken cancellationToken = default
    );
    Task UpdateMotelAsync(
        Guid id,
        string name,
        string description,
        string phoneNumber,
        string email,
        Address? address = null,
        CancellationToken cancellationToken = default
    );
    Task DeleteMotelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> MotelExistsAsync(Guid id, CancellationToken cancellationToken = default);

    // Room management operations
    Task<Room?> GetRoomByIdAsync(
        Guid motelId,
        Guid roomId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Room>> GetRoomsAsync(
        Guid motelId,
        bool availableOnly = false,
        CancellationToken cancellationToken = default
    );
    Task<Room> CreateRoomAsync(
        Guid motelId,
        string roomNumber,
        string name,
        string description,
        RoomType type,
        int capacity,
        Money pricePerHour,
        string? imageUrl = null,
        CancellationToken cancellationToken = default
    );
    Task UpdateRoomAsync(
        Guid motelId,
        Guid roomId,
        string name,
        string description,
        int capacity,
        Money pricePerHour,
        CancellationToken cancellationToken = default
    );
    Task UpdateRoomAvailabilityAsync(
        Guid motelId,
        Guid roomId,
        bool isAvailable,
        CancellationToken cancellationToken = default
    );

    // Room availability methods
    bool IsRoomAvailableForTimeRange(Room room, TimeRange timeRange);
    IEnumerable<Room> GetAvailableRooms(
        IEnumerable<Room> rooms,
        TimeRange timeRange,
        int? capacity = null
    );
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(
        Guid motelId,
        TimeRange timeRange,
        int? capacity = null,
        CancellationToken cancellationToken = default
    );
}
