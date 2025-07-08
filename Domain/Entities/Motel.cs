using Gomotel.Domain.Common;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Motel : AggregateRoot
{
    private readonly List<Room> _rooms = new();

    public string Name { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public Address Address { get; internal set; } = null!;
    public string PhoneNumber { get; internal set; } = string.Empty;
    public string Email { get; internal set; } = string.Empty;
    public Guid OwnerId { get; internal set; } // MotelAdmin user ID
    public bool IsActive { get; internal set; } = true;
    public string? ImageUrl { get; internal set; }

    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    // Private constructor for EF Core
    private Motel() { }

    // Internal constructor for domain services
    internal Motel(
        string name,
        string description,
        Address address,
        string phoneNumber,
        string email,
        Guid ownerId,
        string? imageUrl = null
    )
    {
        Name = name;
        Description = description;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        OwnerId = ownerId;
        ImageUrl = imageUrl;
        IsActive = true;
    }

    // Internal method to update UpdatedAt timestamp
    internal void MarkAsUpdated()
    {
        SetUpdatedAt();
    }

    // Internal method to add rooms (accessible by domain services)
    internal void AddRoomInternal(Room room)
    {
        _rooms.Add(room);
    }
}
