using Microsoft.Extensions.Configuration;
using RabbitMqServices;

namespace Sender;

public class App
{
    private readonly IConfiguration _configuration;
    private readonly IProducer _producer;

    public App(IConfiguration configuration, IProducer producer)
    {
        _configuration = configuration;
        _producer = producer;
    }

    public Task RunAsync()
    {
        Console.Write("Write message: ");
        var message = Console.ReadLine();

        _producer.SendMessage(message!);

        Console.WriteLine("SEND!");
        
        return Task.CompletedTask;
    }
}