using BusPlayground.Api.Bus.Events;
using MassTransit;

namespace BusPlayground.Api.Bus.Sagas.DataTransfer;

public class DataTransferSaga : MassTransitStateMachine<DataTransferState>
{
    public State Scheduled { get; private set; } = null!;
    public State Executing { get; private set; } = null!;

    public Event<InitiateDataTransfer> DataTransferInitiated { get; private set; } = null!;
    public Event<UpdateDataTransferParameters> DataTransferParametersUpdated { get; private set; } = null!;
    public Event<StartDataTransfer> DataTransferStarted { get; private set; } = null!;

    public Schedule<DataTransferState, StartDataTransfer> StartDataTransfer { get; private set; } = null!;

    public DataTransferSaga()
    {
        InstanceState(x => x.CurrentState);
        SetCompletedWhenFinalized();

        Schedule(() => StartDataTransfer, x => x.MigrationStartScheduleId);

        Event(
            () => DataTransferStarted,
            x => x.OnMissingInstance(e => e.Discard())
        );

        Initially(
            When(DataTransferInitiated)
                .Then(x =>
                {
                    x.Saga.Source = x.Message.Source;
                    x.Saga.Target = x.Message.Target;
                    x.Saga.ScheduledDate = x.Message.ScheduledDate;
                })
                .Schedule(StartDataTransfer,
                    x => new StartDataTransfer { ClientId = x.Saga.ClientId },
                    x => x.Saga.ScheduledDate - TimeProvider.System.GetUtcNow())
                .TransitionTo(Scheduled)
        );

        During(Scheduled,
            When(DataTransferStarted).TransitionTo(Executing),
            When(DataTransferParametersUpdated)
                .Then(x =>
                {
                    x.Saga.Source = x.Message.Source;
                    x.Saga.Target = x.Message.Target;
                    x.Saga.ScheduledDate = x.Message.ScheduledDate;
                })
                .Unschedule(StartDataTransfer)
                .Schedule(StartDataTransfer,
                    x => new StartDataTransfer { ClientId = x.Saga.ClientId },
                    x => { return x.Saga.ScheduledDate - TimeProvider.System.GetUtcNow(); })
        );
    }
}