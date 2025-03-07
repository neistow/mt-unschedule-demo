using BusPlayground.Api.Bus.Sagas.DataTransfer;
using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusPlayground.Api.Infrastructure;

public class DataTransferStateConfiguration : SagaClassMap<DataTransferState>
{
    public void Configure(EntityTypeBuilder<DataTransferState> builder)
    {
        builder.Property(x => x.CurrentState).HasMaxLength(64);
        builder.Property(x => x.ScheduledDate);
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(512);
        builder.Property(x => x.Target).HasConversion<string>().HasMaxLength(512);
        builder.Property(x => x.FaultReason).HasMaxLength(1024);
        builder.Property(x => x.MigrationStartScheduleId);
    }
}