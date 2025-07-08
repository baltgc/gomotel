using Gomotel.Domain.Common;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Reservation : AggregateRoot
{
    public Guid MotelId { get; internal set; }
    public Guid RoomId { get; internal set; }
    public Guid UserId { get; internal set; }
    public TimeRange TimeRange { get; internal set; } = null!;
    public ReservationStatus Status { get; internal set; }
    public Money TotalAmount { get; internal set; } = null!;
    public Guid? PaymentId { get; internal set; }
    public string? SpecialRequests { get; internal set; }
    public DateTime? CheckInTime { get; internal set; }
    public DateTime? CheckOutTime { get; internal set; }

    // Navigation properties
    public Motel Motel { get; private set; } = null!;
    public Room Room { get; private set; } = null!;
    public Payment? Payment { get; private set; }

    // Private constructor for EF Core
    private Reservation() { }

    // Internal constructor for domain services
    internal Reservation(
        Guid motelId,
        Guid roomId,
        Guid userId,
        TimeRange timeRange,
        Money totalAmount,
        string? specialRequests = null
    )
    {
        MotelId = motelId;
        RoomId = roomId;
        UserId = userId;
        TimeRange = timeRange;
        Status = ReservationStatus.Pending;
        TotalAmount = totalAmount;
        SpecialRequests = specialRequests;
    }

    // Internal method to update UpdatedAt timestamp
    internal void MarkAsUpdated()
    {
        SetUpdatedAt();
    }

    // Internal method to add domain events (accessible by domain services)
    internal void AddEvent(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}
