using System;

namespace Gomotel.Domain.Exceptions;

/// <summary>
/// Exception thrown when a room is not available for the requested time period
/// </summary>
public class RoomNotAvailableException : DomainException
{
    public Guid RoomId { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public RoomNotAvailableException(Guid roomId, DateTime startTime, DateTime endTime)
        : base(
            $"Room {roomId} is not available from {startTime:yyyy-MM-dd HH:mm} to {endTime:yyyy-MM-dd HH:mm}"
        )
    {
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
    }
}

/// <summary>
/// Exception thrown when a reservation operation violates business rules
/// </summary>
public class InvalidReservationOperationException : DomainException
{
    public Guid ReservationId { get; }
    public string Operation { get; }

    public InvalidReservationOperationException(Guid reservationId, string operation, string reason)
        : base($"Cannot {operation} reservation {reservationId}: {reason}")
    {
        ReservationId = reservationId;
        Operation = operation;
    }
}

/// <summary>
/// Exception thrown when there's a booking conflict with existing reservations
/// </summary>
public class BookingConflictException : DomainException
{
    public Guid RoomId { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public Guid ConflictingReservationId { get; }

    public BookingConflictException(
        Guid roomId,
        DateTime startTime,
        DateTime endTime,
        Guid conflictingReservationId
    )
        : base(
            $"Booking conflict for room {roomId} from {startTime:yyyy-MM-dd HH:mm} to {endTime:yyyy-MM-dd HH:mm}. Conflicts with reservation {conflictingReservationId}"
        )
    {
        RoomId = roomId;
        StartTime = startTime;
        EndTime = endTime;
        ConflictingReservationId = conflictingReservationId;
    }
}

/// <summary>
/// Exception thrown when attempting to book a room with invalid time constraints
/// </summary>
public class InvalidBookingTimeException : DomainException
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public InvalidBookingTimeException(DateTime startTime, DateTime endTime, string reason)
        : base(
            $"Invalid booking time from {startTime:yyyy-MM-dd HH:mm} to {endTime:yyyy-MM-dd HH:mm}: {reason}"
        )
    {
        StartTime = startTime;
        EndTime = endTime;
    }
}

/// <summary>
/// Exception thrown when a reservation cannot be cancelled due to business rules
/// </summary>
public class ReservationCancellationException : DomainException
{
    public Guid ReservationId { get; }

    public ReservationCancellationException(Guid reservationId, string reason)
        : base($"Cannot cancel reservation {reservationId}: {reason}")
    {
        ReservationId = reservationId;
    }
}
