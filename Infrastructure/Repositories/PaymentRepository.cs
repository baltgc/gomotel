using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Repositories;
using Gomotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Gomotel.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context
            .Payments.Include(p => p.Reservation)
            .ThenInclude(r => r.Room)
            .ThenInclude(room => room.Motel)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.ReservationId == reservationId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.Where(p => p.Status == status)
            .Include(p => p.Reservation)
            .ThenInclude(r => r.Room)
            .ThenInclude(room => room.Motel)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.Where(p => p.Reservation.UserId == userId)
            .Include(p => p.Reservation)
            .ThenInclude(r => r.Room)
            .ThenInclude(room => room.Motel)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.Where(p => p.TransactionId == transactionId)
            .Include(p => p.Reservation)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Payments.Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .Include(p => p.Reservation)
            .ThenInclude(r => r.Room)
            .ThenInclude(room => room.Motel)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default
    )
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> HasPendingPaymentForReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Payments.AnyAsync(
            p =>
                p.ReservationId == reservationId
                && (p.Status == PaymentStatus.Created || p.Status == PaymentStatus.Processing),
            cancellationToken
        );
    }
}
