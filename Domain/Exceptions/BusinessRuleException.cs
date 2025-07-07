using System;

namespace Gomotel.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base($"Business rule violation ({ruleName}): {message}")
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when attempting to add a duplicate room number to a motel
/// </summary>
public class DuplicateRoomNumberException : DomainException
{
    public Guid MotelId { get; }
    public string RoomNumber { get; }

    public DuplicateRoomNumberException(Guid motelId, string roomNumber)
        : base($"Room number '{roomNumber}' already exists in motel {motelId}")
    {
        MotelId = motelId;
        RoomNumber = roomNumber;
    }
}

/// <summary>
/// Exception thrown when attempting to operate on an inactive motel
/// </summary>
public class InactiveMotelException : DomainException
{
    public Guid MotelId { get; }

    public InactiveMotelException(Guid motelId)
        : base($"Cannot perform operation on inactive motel {motelId}")
    {
        MotelId = motelId;
    }
}

/// <summary>
/// Exception thrown when room capacity is insufficient for the reservation
/// </summary>
public class InsufficientRoomCapacityException : DomainException
{
    public Guid RoomId { get; }
    public int RequestedCapacity { get; }
    public int AvailableCapacity { get; }

    public InsufficientRoomCapacityException(
        Guid roomId,
        int requestedCapacity,
        int availableCapacity
    )
        : base(
            $"Room {roomId} has capacity for {availableCapacity} guests, but {requestedCapacity} were requested"
        )
    {
        RoomId = roomId;
        RequestedCapacity = requestedCapacity;
        AvailableCapacity = availableCapacity;
    }
}

/// <summary>
/// Exception thrown when a room is marked as unavailable for bookings
/// </summary>
public class RoomUnavailableException : DomainException
{
    public Guid RoomId { get; }

    public RoomUnavailableException(Guid roomId)
        : base($"Room {roomId} is currently unavailable for booking")
    {
        RoomId = roomId;
    }
}

/// <summary>
/// Exception thrown when attempting to modify a room that has active reservations
/// </summary>
public class RoomHasActiveReservationsException : DomainException
{
    public Guid RoomId { get; }
    public int ActiveReservationCount { get; }

    public RoomHasActiveReservationsException(Guid roomId, int activeReservationCount)
        : base(
            $"Cannot modify room {roomId} as it has {activeReservationCount} active reservation(s)"
        )
    {
        RoomId = roomId;
        ActiveReservationCount = activeReservationCount;
    }
}
