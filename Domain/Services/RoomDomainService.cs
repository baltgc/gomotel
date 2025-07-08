using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class RoomDomainService
{
    public Room CreateRoom(
        Guid motelId,
        string roomNumber,
        string name,
        string description,
        RoomType type,
        int capacity,
        Money pricePerHour,
        string? imageUrl = null
    )
    {
        ValidateRoomCreation(motelId, roomNumber, name, capacity, pricePerHour);

        return new Room(
            motelId,
            roomNumber,
            name,
            description,
            type,
            capacity,
            pricePerHour,
            imageUrl
        );
    }

    public void UpdateRoomDetails(
        Room room,
        string name,
        string description,
        int capacity,
        Money pricePerHour
    )
    {
        ValidateRoomUpdate(name, capacity, pricePerHour);

        room.Name = name;
        room.Description = description;
        room.Capacity = capacity;
        room.PricePerHour = pricePerHour;
        room.MarkAsUpdated();
    }

    public void SetRoomAvailability(Room room, bool isAvailable)
    {
        room.IsAvailable = isAvailable;
        room.MarkAsUpdated();
    }

    public bool IsRoomAvailableForTimeRange(Room room, TimeRange timeRange)
    {
        if (!room.IsAvailable)
            return false;

        // Check if there are any overlapping reservations
        var overlappingReservations = room
            .Reservations.Where(r => r.Status != ReservationStatus.Cancelled)
            .Where(r => r.TimeRange.OverlapsWith(timeRange))
            .ToList();

        return !overlappingReservations.Any();
    }

    public IEnumerable<Room> GetAvailableRooms(
        IEnumerable<Room> rooms,
        TimeRange timeRange,
        int? capacity = null
    )
    {
        return rooms.Where(room =>
        {
            if (!room.IsAvailable)
                return false;

            if (capacity.HasValue && room.Capacity < capacity.Value)
                return false;

            return IsRoomAvailableForTimeRange(room, timeRange);
        });
    }

    private void ValidateRoomCreation(
        Guid motelId,
        string roomNumber,
        string name,
        int capacity,
        Money pricePerHour
    )
    {
        if (motelId == Guid.Empty)
            throw new ArgumentException("Motel ID cannot be empty", nameof(motelId));
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number cannot be empty", nameof(roomNumber));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name cannot be empty", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));
        if (pricePerHour == null)
            throw new ArgumentNullException(nameof(pricePerHour));
    }

    private void ValidateRoomUpdate(string name, int capacity, Money pricePerHour)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name cannot be empty", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));
        if (pricePerHour == null)
            throw new ArgumentNullException(nameof(pricePerHour));
    }
}
