using MassTransit;

namespace BusPlayground.Api.Bus.Events;

public record UpdateDataTransferParameters : CorrelatedBy<Guid>
{
    public Guid CorrelationId => ClientId;
    public required Guid ClientId { get; init; }

    public required Uri Source { get; init; }
    public required Uri Target { get; init; }
    public required DateTimeOffset ScheduledDate { get; init; }
}