using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class MotelService : IMotelService
{
    private readonly IMotelRepository _motelRepository;
    private readonly MotelDomainService _motelDomainService;
    private readonly RoomDomainService _roomDomainService;

    public MotelService(
        IMotelRepository motelRepository,
        MotelDomainService motelDomainService,
        RoomDomainService roomDomainService
    )
    {
        _motelRepository = motelRepository;
        _motelDomainService = motelDomainService;
        _roomDomainService = roomDomainService;
    }

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
        var motel = _motelDomainService.CreateMotel(
            name,
            description,
            address,
            phoneNumber,
            email,
            ownerId,
            imageUrl
        );
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

        _motelDomainService.UpdateMotelDetails(motel, name, description, phoneNumber, email);

        if (address != null)
        {
            _motelDomainService.UpdateMotelAddress(motel, address);
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

        var room = _motelDomainService.GetRoomById(motel, roomId);
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

        return availableOnly ? _motelDomainService.GetAvailableRooms(motel) : motel.Rooms;
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

        var room = _roomDomainService.CreateRoom(
            motelId,
            roomNumber,
            name,
            description,
            type,
            capacity,
            pricePerHour,
            imageUrl
        );

        _motelDomainService.AddRoomToMotel(motel, room);
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

        var room = _motelDomainService.GetRoomById(motel, roomId);
        if (room == null)
        {
            throw new RoomNotFoundException(roomId, motelId);
        }

        _roomDomainService.UpdateRoomDetails(room, name, description, capacity, pricePerHour);
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

        var room = _motelDomainService.GetRoomById(motel, roomId);
        if (room == null)
        {
            throw new RoomNotFoundException(roomId, motelId);
        }

        _roomDomainService.SetRoomAvailability(room, isAvailable);
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

        return _roomDomainService.GetAvailableRooms(motel.Rooms, timeRange, capacity);
    }

    public bool IsRoomAvailableForTimeRange(Room room, TimeRange timeRange)
    {
        return _roomDomainService.IsRoomAvailableForTimeRange(room, timeRange);
    }

    public IEnumerable<Room> GetAvailableRooms(
        IEnumerable<Room> rooms,
        TimeRange timeRange,
        int? capacity = null
    )
    {
        return _roomDomainService.GetAvailableRooms(rooms, timeRange, capacity);
    }
}
