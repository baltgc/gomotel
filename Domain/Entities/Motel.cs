using Gomotel.Domain.Common;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Motel : AggregateRoot
{
    private readonly List<Room> _rooms = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; } // MotelAdmin user ID
    public bool IsActive { get; private set; } = true;
    public string? ImageUrl { get; private set; }

    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    private Motel() { } // EF Core constructor

    public static Motel Create(
        string name,
        string description,
        Address address,
        string phoneNumber,
        string email,
        Guid ownerId,
        string? imageUrl = null
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
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));

        var motel = new Motel
        {
            Name = name,
            Description = description,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email,
            OwnerId = ownerId,
            ImageUrl = imageUrl,
        };

        return motel;
    }

    public void UpdateDetails(string name, string description, string phoneNumber, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Motel name cannot be empty", nameof(name));

        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        SetUpdatedAt();
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        SetUpdatedAt();
    }

    public void AddRoom(Room room)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));
        if (_rooms.Any(r => r.RoomNumber == room.RoomNumber))
            throw new DuplicateRoomNumberException(Id, room.RoomNumber);

        _rooms.Add(room);
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
