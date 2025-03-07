using MassTransit;

namespace BusPlayground.Api.Bus.Events;

public record StartDataTransfer : CorrelatedBy<Guid>
{
    public Guid CorrelationId => ClientId;
    public required Guid ClientId { get; init; }
}