using System;

namespace Gomotel.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user lacks authorization to perform an operation
/// </summary>
public class UnauthorizedOperationException : DomainException
{
    public Guid UserId { get; }
    public string Operation { get; }
    public string Resource { get; }

    public UnauthorizedOperationException(Guid userId, string operation, string resource)
        : base($"User {userId} is not authorized to {operation} on {resource}")
    {
        UserId = userId;
        Operation = operation;
        Resource = resource;
    }
}

/// <summary>
/// Exception thrown when a motel admin tries to access resources they don't own
/// </summary>
public class MotelAccessDeniedException : DomainException
{
    public Guid UserId { get; }
    public Guid MotelId { get; }

    public MotelAccessDeniedException(Guid userId, Guid motelId)
        : base($"User {userId} does not have access to motel {motelId}")
    {
        UserId = userId;
        MotelId = motelId;
    }
}

/// <summary>
/// Exception thrown when a user tries to access a reservation they don't own
/// </summary>
public class ReservationAccessDeniedException : DomainException
{
    public Guid UserId { get; }
    public Guid ReservationId { get; }

    public ReservationAccessDeniedException(Guid userId, Guid reservationId)
        : base($"User {userId} does not have access to reservation {reservationId}")
    {
        UserId = userId;
        ReservationId = reservationId;
    }
}

/// <summary>
/// Exception thrown when user account is not in valid state for the operation
/// </summary>
public class InvalidUserStateException : DomainException
{
    public Guid UserId { get; }
    public string UserState { get; }

    public InvalidUserStateException(Guid userId, string userState, string requiredState)
        : base(
            $"User {userId} is in '{userState}' state, but '{requiredState}' state is required for this operation"
        )
    {
        UserId = userId;
        UserState = userState;
    }
}

/// <summary>
/// Exception thrown when user's role is insufficient for the operation
/// </summary>
public class InsufficientRoleException : DomainException
{
    public Guid UserId { get; }
    public string CurrentRole { get; }
    public string RequiredRole { get; }

    public InsufficientRoleException(Guid userId, string currentRole, string requiredRole)
        : base(
            $"User {userId} has role '{currentRole}', but '{requiredRole}' role is required for this operation"
        )
    {
        UserId = userId;
        CurrentRole = currentRole;
        RequiredRole = requiredRole;
    }
}
