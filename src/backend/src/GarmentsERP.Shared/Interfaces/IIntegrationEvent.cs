namespace GarmentsERP.Shared.Interfaces;

/// <summary>
/// Base contract for asynchronous cross-module integration events published over RabbitMQ / MassTransit.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
