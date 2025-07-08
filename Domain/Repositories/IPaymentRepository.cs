using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;

namespace Gomotel.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Payment>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Payment>> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Payment>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasPendingPaymentForReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default
    );
}
