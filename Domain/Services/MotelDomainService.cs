using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class MotelDomainService
{
    public Motel CreateMotel(
        string name,
        string description,
        Address address,
        string phoneNumber,
        string email,
        Guid ownerId,
        string? imageUrl = null
    )
    {
        ValidateMotelCreation(name, description, address, phoneNumber, email, ownerId);

        return new Motel(name, description, address, phoneNumber, email, ownerId, imageUrl);
    }

    public void UpdateMotelDetails(
        Motel motel,
        string name,
        string description,
        string phoneNumber,
        string email
    )
    {
        ValidateMotelUpdate(name, description, phoneNumber, email);

        motel.Name = name;
        motel.Description = description;
        motel.PhoneNumber = phoneNumber;
        motel.Email = email;
        motel.MarkAsUpdated();
    }

    public void UpdateMotelAddress(Motel motel, Address address)
    {
        if (address == null)
            throw new ArgumentNullException(nameof(address));

        motel.Address = address;
        motel.MarkAsUpdated();
    }

    public void AddRoomToMotel(Motel motel, Room room)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        ValidateRoomAddition(motel, room);

        motel.AddRoomInternal(room);
        motel.MarkAsUpdated();
    }

    public void DeactivateMotel(Motel motel)
    {
        motel.IsActive = false;
        motel.MarkAsUpdated();
    }

    public void ActivateMotel(Motel motel)
    {
        motel.IsActive = true;
        motel.MarkAsUpdated();
    }

    public bool CanMotelBeDeleted(Motel motel)
    {
        // A motel can be deleted if it has no active reservations
        return !motel.Rooms.Any(room =>
            room.Reservations.Any(reservation =>
                reservation.Status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn
            )
        );
    }

    public bool IsRoomNumberAvailable(Motel motel, string roomNumber)
    {
        return !motel.Rooms.Any(r => r.RoomNumber == roomNumber);
    }

    public Room? GetRoomByNumber(Motel motel, string roomNumber)
    {
        return motel.Rooms.FirstOrDefault(r => r.RoomNumber == roomNumber);
    }

    public Room? GetRoomById(Motel motel, Guid roomId)
    {
        return motel.Rooms.FirstOrDefault(r => r.Id == roomId);
    }

    public IEnumerable<Room> GetAvailableRooms(Motel motel)
    {
        return motel.Rooms.Where(r => r.IsAvailable);
    }

    public int GetTotalRoomCount(Motel motel)
    {
        return motel.Rooms.Count;
    }

    public int GetAvailableRoomCount(Motel motel)
    {
        return motel.Rooms.Count(r => r.IsAvailable);
    }

    private void ValidateMotelCreation(
        string name,
        string description,
        Address address,
        string phoneNumber,
        string email,
        Guid ownerId
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Motel name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
        if (address == null)
            throw new ArgumentNullException(nameof(address));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));
    }

    private void ValidateMotelUpdate(
        string name,
        string description,
        string phoneNumber,
        string email
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Motel name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
    }

    private void ValidateRoomAddition(Motel motel, Room room)
    {
        if (motel.Rooms.Any(r => r.RoomNumber == room.RoomNumber))
            throw new DuplicateRoomNumberException(motel.Id, room.RoomNumber);
    }
}
