using System.Net;
using System.Text.Json;
using Gomotel.Domain.Exceptions;

namespace Gomotel.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware that provides consistent error responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            // Entity Not Found Exceptions
            EntityNotFoundException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = ex.Message,
                Details = $"{ex.EntityName} with ID '{ex.EntityId}' was not found",
                ErrorType = "EntityNotFound",
            },

            // Reservation Exceptions
            RoomNotAvailableException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details =
                    $"Room {ex.RoomId} is not available from {ex.StartTime:yyyy-MM-dd HH:mm} to {ex.EndTime:yyyy-MM-dd HH:mm}",
                ErrorType = "RoomNotAvailable",
            },

            BookingConflictException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = ex.Message,
                Details =
                    $"Time slot conflicts with existing reservation {ex.ConflictingReservationId}",
                ErrorType = "BookingConflict",
            },

            InvalidBookingTimeException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details =
                    $"Invalid booking time from {ex.StartTime:yyyy-MM-dd HH:mm} to {ex.EndTime:yyyy-MM-dd HH:mm}",
                ErrorType = "InvalidBookingTime",
            },

            InvalidReservationOperationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = $"Cannot {ex.Operation} reservation {ex.ReservationId}",
                ErrorType = "InvalidReservationOperation",
            },

            ReservationCancellationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = $"Cannot cancel reservation {ex.ReservationId}",
                ErrorType = "ReservationCancellationError",
            },

            // Business Rule Exceptions
            DuplicateRoomNumberException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = ex.Message,
                Details = $"Room number '{ex.RoomNumber}' already exists in motel {ex.MotelId}",
                ErrorType = "DuplicateRoomNumber",
            },

            InactiveMotelException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = $"Motel {ex.MotelId} is inactive",
                ErrorType = "InactiveMotel",
            },

            RoomUnavailableException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = $"Room {ex.RoomId} is currently unavailable",
                ErrorType = "RoomUnavailable",
            },

            InsufficientRoomCapacityException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details =
                    $"Room capacity {ex.AvailableCapacity} is less than requested {ex.RequestedCapacity}",
                ErrorType = "InsufficientRoomCapacity",
            },

            // Payment Exceptions
            PaymentProcessingException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.PaymentRequired,
                Message = ex.Message,
                Details = $"Payment {ex.PaymentId} failed using {ex.PaymentMethod}",
                ErrorType = "PaymentProcessingError",
            },

            InvalidPaymentAmountException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details =
                    $"Expected {ex.ExpectedAmount:C} {ex.Currency}, but received {ex.RequestedAmount:C}",
                ErrorType = "InvalidPaymentAmount",
            },

            // Authorization Exceptions
            UnauthorizedOperationException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = ex.Message,
                Details = $"User {ex.UserId} cannot {ex.Operation} on {ex.Resource}",
                ErrorType = "UnauthorizedOperation",
            },

            MotelAccessDeniedException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = ex.Message,
                Details = $"User {ex.UserId} does not have access to motel {ex.MotelId}",
                ErrorType = "MotelAccessDenied",
            },

            ReservationAccessDeniedException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = ex.Message,
                Details =
                    $"User {ex.UserId} does not have access to reservation {ex.ReservationId}",
                ErrorType = "ReservationAccessDenied",
            },

            // Generic Domain Exception
            DomainException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = "A business rule violation occurred",
                ErrorType = "DomainError",
            },

            // Validation Exceptions
            ArgumentException ex => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ex.Message,
                Details = "Invalid input provided",
                ErrorType = "ValidationError",
            },

            // Default case for unhandled exceptions
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred",
                Details = "Please try again later or contact support if the problem persists",
                ErrorType = "InternalServerError",
            },
        };

        // Log the exception
        if (response.StatusCode >= 500)
        {
            _logger.LogError(exception, "Internal server error occurred");
        }
        else if (response.StatusCode >= 400)
        {
            _logger.LogWarning(exception, "Client error occurred: {Message}", exception.Message);
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standard error response model
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
}
