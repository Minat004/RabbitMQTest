using RabbitMqShared.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json");
        config.AddEnvironmentVariables();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Consumer>();
        services.AddSingleton<IMailService, MailService>();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();