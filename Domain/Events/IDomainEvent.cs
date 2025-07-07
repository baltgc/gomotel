using MediatR;

namespace Gomotel.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
