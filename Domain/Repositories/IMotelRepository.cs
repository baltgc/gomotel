using Gomotel.Domain.Entities;

namespace Gomotel.Domain.Repositories;

public interface IMotelRepository
{
    Task<Motel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Motel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Motel>> GetByOwnerIdAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default
    );
    Task<Motel> AddAsync(Motel motel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Motel motel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Motel motel, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
