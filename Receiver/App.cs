using Microsoft.Extensions.Configuration;
using RabbitMqServices;

namespace Receiver;

public class App
{
    private readonly IConfiguration _configuration;

    public App(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task RunAsync()
    {
        // var consumer = new Consumer(_configuration);
        //
        // consumer.ReceiveMessage();

        Console.ReadLine();
        
        return Task.CompletedTask;
    }
}