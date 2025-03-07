using MassTransit;

namespace BusPlayground.Api.Bus.Sagas.DataTransfer;

public class DataTransferState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;

    public Guid ClientId => CorrelationId;
    public required Uri Source { get; set; }
    public required Uri Target { get; set; }

    public required DateTimeOffset ScheduledDate { get; set; }

    public string? FaultReason { get; set; }

    public Guid? MigrationStartScheduleId { get; set; }
    

    public Guid? MigrationStartConcurrencyToken { get; set; }
}