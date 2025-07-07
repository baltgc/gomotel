using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.Repositories;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMotelRepository _motelRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        IMotelRepository motelRepository
    )
    {
        _reservationRepository = reservationRepository;
        _motelRepository = motelRepository;
    }

    // Reservation CRUD operations
    public async Task<Reservation?> GetReservationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _reservationRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _reservationRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByMotelIdAsync(
        Guid motelId,
        CancellationToken cancellationToken = default
    )
    {
        return await _reservationRepository.GetByMotelIdAsync(motelId, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByRoomIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default
    )
    {
        return await _reservationRepository.GetByRoomIdAsync(roomId, cancellationToken);
    }

    public async Task<Reservation> CreateReservationAsync(
        Guid motelId,
        Guid roomId,
        Guid userId,
        DateTime startTime,
        DateTime endTime,
        string? specialRequests = null,
        CancellationToken cancellationToken = default
    )
    {
        // Validate the motel and room exist
        var motel = await _motelRepository.GetByIdAsync(motelId, cancellationToken);
        if (motel == null)
        {
            throw new MotelNotFoundException(motelId);
        }

        var room = motel.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            throw new RoomNotFoundException(roomId, motelId);
        }

        if (!room.IsAvailable)
        {
            throw new RoomUnavailableException(roomId);
        }

        // Create time range and validate
        var timeRange = TimeRange.Create(startTime, endTime);

        // Check for overlapping reservations
        var hasOverlapping = await _reservationRepository.HasOverlappingReservationAsync(
            roomId,
            timeRange,
            null,
            cancellationToken
        );

        if (hasOverlapping)
        {
            throw new BookingConflictException(roomId, startTime, endTime, Guid.Empty);
        }

        // Calculate total amount
        var duration = timeRange.Duration;
        var totalAmount = Money.Create(
            room.PricePerHour.Amount * (decimal)duration.TotalHours,
            room.PricePerHour.Currency
        );

        var reservation = Reservation.Create(
            motelId,
            roomId,
            userId,
            timeRange,
            totalAmount,
            specialRequests
        );

        return await _reservationRepository.AddAsync(reservation, cancellationToken);
    }

    public async Task UpdateReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    )
    {
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
    }

    public async Task DeleteReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default
    )
    {
        await _reservationRepository.DeleteAsync(reservation, cancellationToken);
    }

    // Reservation validation
    public async Task<bool> HasOverlappingReservationAsync(
        Guid roomId,
        TimeRange timeRange,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _reservationRepository.HasOverlappingReservationAsync(
            roomId,
            timeRange,
            excludeReservationId,
            cancellationToken
        );
    }

    // Reservation state management
    public void ConfirmReservation(Reservation reservation)
    {
        if (reservation.Status != ReservationStatus.Pending)
            throw new InvalidReservationOperationException(
                reservation.Id,
                "confirm",
                "Only pending reservations can be confirmed"
            );

        reservation.SetConfirmed();
    }

    public void CheckInReservation(Reservation reservation)
    {
        if (reservation.Status != ReservationStatus.Confirmed)
            throw new InvalidReservationOperationException(
                reservation.Id,
                "check in",
                "Only confirmed reservations can be checked in"
            );

        reservation.SetCheckedIn();
    }

    public void CheckOutReservation(Reservation reservation)
    {
        if (reservation.Status != ReservationStatus.CheckedIn)
            throw new InvalidReservationOperationException(
                reservation.Id,
                "check out",
                "Only checked-in reservations can be checked out"
            );

        reservation.SetCheckedOut();
    }

    public void CancelReservation(Reservation reservation)
    {
        if (reservation.Status is ReservationStatus.CheckedOut or ReservationStatus.Cancelled)
            throw new ReservationCancellationException(
                reservation.Id,
                "Cannot cancel completed or already cancelled reservations"
            );

        reservation.SetCancelled();
    }
}
