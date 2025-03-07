Steps to reproduce:

1) Specify your Postgres connection string in `appsettings.Development.json`
2) Run `dotnet tool restore`
3) Run `dotnet ef database update`
4) Run application
5) Open the application and make a GET request to the endpoint `/start` - this will start saga and return `id` - copy it. Observe saga state in DB - state will be `Scheduled`.
6) The first event is scheduled 30 seconds ahead, so you should be relatively quick and navigate to `/change?id={id_you_copied}` this will send `DataTransferParametersUpdated` with a new scheduled date for an hour later
7) Wait for a 30seconds. Observe saga state in database - it will be `Executing`
8) In saga upon receiving `DataTransferParametersUpdated` old schedule is unscheduled, but event still arrives and triggers state change
9) Expected result is that saga should be still in `Scheduled` state until newly rescheduled events arrives
