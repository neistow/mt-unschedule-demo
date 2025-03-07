using MassTransit;
using BusPlayground.Api;
using BusPlayground.Api.Bus.Events;
using BusPlayground.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddNpgsql<AppDbContext>(builder.Configuration.GetConnectionString("Database"));

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
    );

    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        x.AddDelayedMessageScheduler();

        x.SetEntityFrameworkSagaRepositoryProvider(o =>
        {
            o.UsePostgres();
            o.ExistingDbContext<AppDbContext>();
        });

        x.AddEntityFrameworkOutbox<AppDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        x.AddConsumers(AssemblyMarker.Assembly);
        x.AddActivities(AssemblyMarker.Assembly);
        x.AddSagaStateMachines(AssemblyMarker.Assembly);

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.UseDelayedMessageScheduler();

            cfg.UseMessageRetry(r => r.Interval(3, 200));

            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ConfigureEndpoints(context);
        });

        x.AddConfigureEndpointsCallback((context, _, cfg) => { cfg.UseEntityFrameworkOutbox<AppDbContext>(context); });
    });

    var app = builder.Build();

    app.MapGet("/start",
        async ([FromServices] IPublishEndpoint publishEndpoint, [FromServices] AppDbContext dbContext) =>
        {
            var id = Guid.CreateVersion7();

            await publishEndpoint.Publish(new InitiateDataTransfer
            {
                ClientId = id,
                Source = new Uri("source://source"),
                Target = new Uri("source://target"),
                ScheduledDate = TimeProvider.System.GetUtcNow().AddSeconds(30)
            });
            await dbContext.SaveChangesAsync();

            return TypedResults.Ok(id);
        });

    app.MapGet("/change",
        async (
            [FromQuery] Guid id,
            [FromServices] IPublishEndpoint publishEndpoint,
            [FromServices] AppDbContext dbContext) =>
        {
            await publishEndpoint.Publish(new UpdateDataTransferParameters
            {
                ClientId = id,
                Source = new Uri("source://source"),
                Target = new Uri("source://target"),
                ScheduledDate = TimeProvider.System.GetUtcNow().AddHours(1),
            });
            await dbContext.SaveChangesAsync();

            return TypedResults.Ok();
        });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}