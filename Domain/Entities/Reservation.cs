using Gomotel.Domain.Common;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Entities;

public class Reservation : AggregateRoot
{
    public Guid MotelId { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public TimeRange TimeRange { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public Guid? PaymentId { get; private set; }
    public string? SpecialRequests { get; private set; }
    public DateTime? CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }

    // Navigation properties
    public Motel Motel { get; private set; } = null!;
    public Room Room { get; private set; } = null!;
    public Payment? Payment { get; private set; }

    private Reservation() { } // EF Core constructor

    public static Reservation Create(
        Guid motelId,
        Guid roomId,
        Guid userId,
        TimeRange timeRange,
        Money totalAmount,
        string? specialRequests = null
    )
    {
        if (motelId == Guid.Empty)
            throw new ArgumentException("Motel ID cannot be empty", nameof(motelId));
        if (roomId == Guid.Empty)
            throw new ArgumentException("Room ID cannot be empty", nameof(roomId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        if (timeRange == null)
            throw new ArgumentNullException(nameof(timeRange));
        if (totalAmount == null)
            throw new ArgumentNullException(nameof(totalAmount));

        var reservation = new Reservation
        {
            MotelId = motelId,
            RoomId = roomId,
            UserId = userId,
            TimeRange = timeRange,
            Status = ReservationStatus.Pending,
            TotalAmount = totalAmount,
            SpecialRequests = specialRequests,
        };

        reservation.AddDomainEvent(
            new ReservationCreatedEvent(reservation.Id, userId, motelId, roomId)
        );

        return reservation;
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Only pending reservations can be confirmed");

        Status = ReservationStatus.Confirmed;
        SetUpdatedAt();
    }

    public void CheckIn()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed reservations can be checked in");

        Status = ReservationStatus.CheckedIn;
        CheckInTime = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void CheckOut()
    {
        if (Status != ReservationStatus.CheckedIn)
            throw new InvalidOperationException("Only checked-in reservations can be checked out");

        Status = ReservationStatus.CheckedOut;
        CheckOutTime = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status is ReservationStatus.CheckedOut or ReservationStatus.Cancelled)
            throw new InvalidOperationException(
                "Cannot cancel completed or already cancelled reservations"
            );

        Status = ReservationStatus.Cancelled;
        SetUpdatedAt();

        AddDomainEvent(new ReservationCancelledEvent(Id, UserId));
    }

    public void AssignPayment(Guid paymentId)
    {
        PaymentId = paymentId;
        SetUpdatedAt();
    }
}
