namespace GarmentsERP.Shared.Interfaces;

/// <summary>
/// Marker interface for synchronous in-process domain events raised by aggregates.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
