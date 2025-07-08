using Gomotel.Domain.Common;
using Gomotel.Domain.Enums;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Room : BaseEntity
{
    private readonly List<Reservation> _reservations = new();

    public Guid MotelId { get; internal set; }
    public string RoomNumber { get; internal set; } = string.Empty;
    public string Name { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public RoomType Type { get; internal set; }
    public int Capacity { get; internal set; }
    public Money PricePerHour { get; internal set; } = null!;
    public bool IsAvailable { get; internal set; } = true;
    public string? ImageUrl { get; internal set; }

    // Navigation properties
    public Motel Motel { get; private set; } = null!;
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    // Private constructor for EF Core
    private Room() { }

    // Internal constructor for domain services
    internal Room(
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
        MotelId = motelId;
        RoomNumber = roomNumber;
        Name = name;
        Description = description;
        Type = type;
        Capacity = capacity;
        PricePerHour = pricePerHour;
        ImageUrl = imageUrl;
        IsAvailable = true;
    }

    // Internal method to update UpdatedAt timestamp
    internal void MarkAsUpdated()
    {
        SetUpdatedAt();
    }
}
