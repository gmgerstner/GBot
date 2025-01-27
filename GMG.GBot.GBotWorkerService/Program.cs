using GMG.GBot.GBotWorkerService;
using GMG.GBot.Library;
using Serilog;
using Serilog.Sinks.MSSqlServer;

IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

Log.Logger = new LoggerConfiguration()
    .WriteTo
    .MSSqlServer(
        connectionString: configuration.GetConnectionString("LogDB"),
        sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs" })
    .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Day) // Add file logging
        .CreateLogger();

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "GBot Service";
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<GBotService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .Build();

await host.RunAsync();
