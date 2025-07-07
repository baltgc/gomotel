using Gomotel.Domain.Entities;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public interface IReservationService
{
    // Reservation CRUD operations
    Task<Reservation?> GetReservationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Reservation>> GetReservationsByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Reservation>> GetReservationsByRoomIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default
    );
    Task<Reservation> CreateReservationAsync(
        Guid motelId,
        Guid roomId,
        Guid userId,
        DateTime startTime,
        DateTime endTime,
        string? specialRequests = null,
        CancellationToken cancellationToken = default
    );
    Task UpdateReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    );
    Task DeleteReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    );

    // Reservation validation
    Task<bool> HasOverlappingReservationAsync(
        Guid roomId,
        TimeRange timeRange,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default
    );

    // Reservation state management
    void ConfirmReservation(Reservation reservation);
    void CheckInReservation(Reservation reservation);
    void CheckOutReservation(Reservation reservation);
    void CancelReservation(Reservation reservation);
}
