using GarmentsERP.Shared.Interfaces;

namespace GarmentsERP.Shared.Events;

/// <summary>
/// Abstract base class for all enterprise integration events transmitted over RabbitMQ / MassTransit.
/// </summary>
public abstract class BaseIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public virtual string EventType => GetType().Name;
}
