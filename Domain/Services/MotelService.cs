using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class MotelService : IMotelService
{
    private readonly IMotelRepository _motelRepository;

    public MotelService(IMotelRepository motelRepository)
    {
        _motelRepository = motelRepository;
    }

    // Motel CRUD operations
    public async Task<IEnumerable<Motel>> GetAllMotelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _motelRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Motel?> GetMotelByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _motelRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Motel>> GetMotelsByOwnerIdAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _motelRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
    }

    public async Task<Motel> CreateMotelAsync(
        string name,
        string description,
        Address address,
        string phoneNumber,
        string email,
        Guid ownerId,
        string? imageUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        var motel = Motel.Create(name, description, address, phoneNumber, email, ownerId, imageUrl);
        return await _motelRepository.AddAsync(motel, cancellationToken);
    }

    public async Task UpdateMotelAsync(
        Guid id,
        string name,
        string description,
        string phoneNumber,
        string email,
        Address? address = null,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(id, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(id);
        }

        motel.UpdateDetails(name, description, phoneNumber, email);

        if (address != null)
        {
            motel.UpdateAddress(address);
        }

        await _motelRepository.UpdateAsync(motel, cancellationToken);
    }

    public async Task DeleteMotelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var motel = await _motelRepository.GetByIdAsync(id, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(id);
        }

        await _motelRepository.DeleteAsync(motel, cancellationToken);
    }

    public async Task<bool> MotelExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _motelRepository.ExistsAsync(id, cancellationToken);
    }

    // Room management operations
    public async Task<Room?> GetRoomByIdAsync(
        Guid motelId,
        Guid roomId,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            throw new RoomNotFoundException(roomId, motelId);
        }

        return room;
    }

    public async Task<IEnumerable<Room>> GetRoomsAsync(
        Guid motelId,
        bool availableOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        return availableOnly ? motel.Rooms.Where(r => r.IsAvailable) : motel.Rooms;
    }

    public async Task<Room> CreateRoomAsync(
        Guid motelId,
        string roomNumber,
        string name,
        string description,
        RoomType type,
        int capacity,
        Money pricePerHour,
        string? imageUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        var room = Room.Create(
            motelId,
            roomNumber,
            name,
            description,
            type,
            capacity,
            pricePerHour,
            imageUrl
        );
        motel.AddRoom(room);
        await _motelRepository.UpdateAsync(motel, cancellationToken);

        return room;
    }

    public async Task UpdateRoomAsync(
        Guid motelId,
        Guid roomId,
        string name,
        string description,
        int capacity,
        Money pricePerHour,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            throw new RoomNotFoundException(roomId, motelId);
        }

        room.UpdateDetails(name, description, capacity, pricePerHour);
        await _motelRepository.UpdateAsync(motel, cancellationToken);
    }

    public async Task UpdateRoomAvailabilityAsync(
        Guid motelId,
        Guid roomId,
        bool isAvailable,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            throw new RoomNotFoundException(roomId, motelId);
        }

        room.SetAvailability(isAvailable);
        await _motelRepository.UpdateAsync(motel, cancellationToken);
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
        Guid motelId,
        TimeRange timeRange,
        int? capacity = null,
        CancellationToken cancellationToken = default
    )
    {
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        return GetAvailableRooms(motel.Rooms, timeRange, capacity);
    }

    // Room availability methods
    public bool IsRoomAvailableForTimeRange(Room room, TimeRange timeRange)
    {
        if (!room.IsAvailable)
            return false;

        // Check for overlapping reservations
        var overlappingReservations = room.Reservations.Where(r =>
            r.Status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn
            && r.TimeRange.OverlapsWith(timeRange)
        );

        return !overlappingReservations.Any();
    }

    public IEnumerable<Room> GetAvailableRooms(
        IEnumerable<Room> rooms,
        TimeRange timeRange,
        int? capacity = null
    )
    {
        var availableRooms = new List<Room>();

        foreach (var room in rooms.Where(r => r.IsAvailable))
        {
            if (capacity.HasValue && room.Capacity < capacity.Value)
                continue;

            var isAvailable = IsRoomAvailableForTimeRange(room, timeRange);
            if (isAvailable)
            {
                availableRooms.Add(room);
            }
        }

        return availableRooms;
    }
}
