using System;

namespace Gomotel.Domain.Exceptions;

/// <summary>
/// Base exception for entity not found scenarios
/// </summary>
public abstract class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    protected EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when a motel is not found
/// </summary>
public class MotelNotFoundException : EntityNotFoundException
{
    public MotelNotFoundException(Guid motelId)
        : base("Motel", motelId) { }
}

/// <summary>
/// Exception thrown when a room is not found
/// </summary>
public class RoomNotFoundException : EntityNotFoundException
{
    public Guid MotelId { get; }

    public RoomNotFoundException(Guid roomId)
        : base("Room", roomId)
    {
        MotelId = Guid.Empty;
    }

    public RoomNotFoundException(Guid roomId, Guid motelId)
        : base("Room", $"{roomId} in motel {motelId}")
    {
        MotelId = motelId;
    }
}

/// <summary>
/// Exception thrown when a reservation is not found
/// </summary>
public class ReservationNotFoundException : EntityNotFoundException
{
    public ReservationNotFoundException(Guid reservationId)
        : base("Reservation", reservationId) { }
}

/// <summary>
/// Exception thrown when a user is not found
/// </summary>
public class UserNotFoundException : EntityNotFoundException
{
    public UserNotFoundException(Guid userId)
        : base("User", userId) { }

    public UserNotFoundException(string email)
        : base("User", email) { }
}

/// <summary>
/// Exception thrown when a payment is not found
/// </summary>
public class PaymentNotFoundException : EntityNotFoundException
{
    public PaymentNotFoundException(Guid paymentId)
        : base("Payment", paymentId) { }
}
