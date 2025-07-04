using Gomotel.Domain.Common;
using Gomotel.Domain.Enums;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Room : BaseEntity
{
    private readonly List<Reservation> _reservations = new();

    public Guid MotelId { get; private set; }
    public string RoomNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RoomType Type { get; private set; }
    public int Capacity { get; private set; }
    public Money PricePerHour { get; private set; } = null!;
    public bool IsAvailable { get; private set; } = true;
    public string? ImageUrl { get; private set; }

    // Navigation properties
    public Motel Motel { get; private set; } = null!;
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Room() { } // EF Core constructor

    public static Room Create(
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

        var room = new Room
        {
            MotelId = motelId,
            RoomNumber = roomNumber,
            Name = name,
            Description = description,
            Type = type,
            Capacity = capacity,
            PricePerHour = pricePerHour,
            ImageUrl = imageUrl,
        };

        return room;
    }

    public void UpdateDetails(string name, string description, int capacity, Money pricePerHour)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name cannot be empty", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero", nameof(capacity));

        Name = name;
        Description = description;
        Capacity = capacity;
        PricePerHour = pricePerHour ?? throw new ArgumentNullException(nameof(pricePerHour));
        SetUpdatedAt();
    }

    public bool IsAvailableForTimeRange(TimeRange timeRange)
    {
        if (!IsAvailable)
            return false;

        return !_reservations.Any(r =>
            r.Status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn
            && r.TimeRange.OverlapsWith(timeRange)
        );
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        SetUpdatedAt();
    }
}
