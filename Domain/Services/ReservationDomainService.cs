using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class ReservationDomainService
{
    public Reservation CreateReservation(
        Guid motelId,
        Guid roomId,
        Guid userId,
        TimeRange timeRange,
        Money totalAmount,
        string? specialRequests = null
    )
    {
        ValidateReservationCreation(motelId, roomId, userId, timeRange, totalAmount);

        var reservation = new Reservation(
            motelId,
            roomId,
            userId,
            timeRange,
            totalAmount,
            specialRequests
        );

        reservation.AddEvent(new ReservationCreatedEvent(reservation.Id, userId, motelId, roomId));

        return reservation;
    }

    public void ConfirmReservation(Reservation reservation)
    {
        ValidateReservationStatusTransition(reservation, ReservationStatus.Pending, "confirm");

        reservation.Status = ReservationStatus.Confirmed;
        reservation.MarkAsUpdated();
    }

    public void CheckInReservation(Reservation reservation)
    {
        ValidateReservationStatusTransition(reservation, ReservationStatus.Confirmed, "check in");

        reservation.Status = ReservationStatus.CheckedIn;
        reservation.CheckInTime = DateTime.UtcNow;
        reservation.MarkAsUpdated();
    }

    public void CheckOutReservation(Reservation reservation)
    {
        ValidateReservationStatusTransition(reservation, ReservationStatus.CheckedIn, "check out");

        reservation.Status = ReservationStatus.CheckedOut;
        reservation.CheckOutTime = DateTime.UtcNow;
        reservation.MarkAsUpdated();
    }

    public void CancelReservation(Reservation reservation)
    {
        ValidateReservationCancellation(reservation);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.MarkAsUpdated();
        reservation.AddEvent(new ReservationCancelledEvent(reservation.Id, reservation.UserId));
    }

    public void AssignPayment(Reservation reservation, Guid paymentId)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("Payment ID cannot be empty", nameof(paymentId));

        reservation.PaymentId = paymentId;
        reservation.MarkAsUpdated();
    }

    public Money CalculateTotalAmount(Room room, TimeRange timeRange)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));
        if (timeRange == null)
            throw new ArgumentNullException(nameof(timeRange));

        var duration = timeRange.Duration;
        return Money.Create(
            room.PricePerHour.Amount * (decimal)duration.TotalHours,
            room.PricePerHour.Currency
        );
    }

    public bool CanReservationBeModified(Reservation reservation)
    {
        return reservation.Status == ReservationStatus.Pending;
    }

    public bool CanReservationBeCancelled(Reservation reservation)
    {
        return reservation.Status
            is not (ReservationStatus.CheckedOut or ReservationStatus.Cancelled);
    }

    private void ValidateReservationCreation(
        Guid motelId,
        Guid roomId,
        Guid userId,
        TimeRange timeRange,
        Money totalAmount
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
    }

    private void ValidateReservationStatusTransition(
        Reservation reservation,
        ReservationStatus expectedStatus,
        string operation
    )
    {
        if (reservation.Status != expectedStatus)
            throw new InvalidReservationOperationException(
                reservation.Id,
                operation,
                $"Only {expectedStatus.ToString().ToLower()} reservations can be {operation}ed"
            );
    }

    private void ValidateReservationCancellation(Reservation reservation)
    {
        if (reservation.Status is ReservationStatus.CheckedOut or ReservationStatus.Cancelled)
            throw new ReservationCancellationException(
                reservation.Id,
                "Cannot cancel completed or already cancelled reservations"
            );
    }
}
