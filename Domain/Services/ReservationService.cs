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
    private readonly ReservationDomainService _reservationDomainService;
    private readonly RoomDomainService _roomDomainService;

    public ReservationService(
        IReservationRepository reservationRepository,
        IMotelRepository motelRepository,
        ReservationDomainService reservationDomainService,
        RoomDomainService roomDomainService
    )
    {
        _reservationRepository = reservationRepository;
        _motelRepository = motelRepository;
        _reservationDomainService = reservationDomainService;
        _roomDomainService = roomDomainService;
    }

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

        // Calculate total amount using domain service
        var totalAmount = _reservationDomainService.CalculateTotalAmount(room, timeRange);

        var reservation = _reservationDomainService.CreateReservation(
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

    public void ConfirmReservation(Reservation reservation)
    {
        _reservationDomainService.ConfirmReservation(reservation);
    }

    public void CheckInReservation(Reservation reservation)
    {
        _reservationDomainService.CheckInReservation(reservation);
    }

    public void CheckOutReservation(Reservation reservation)
    {
        _reservationDomainService.CheckOutReservation(reservation);
    }

    public void CancelReservation(Reservation reservation)
    {
        _reservationDomainService.CancelReservation(reservation);
    }
}
