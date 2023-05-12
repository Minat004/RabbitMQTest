using RabbitMqServices;

namespace Sender;

public class App
{
    private readonly IProducer _producer;

    public App(IProducer producer)
    {
        _producer = producer;
    }

    public Task RunAsync()
    {
        Console.WriteLine("-=(for quit write 'quit')=-");
        
        while (true)
        {
            Console.Write("Write message: ");
            var message = Console.ReadLine();

            if (message == "quit")
            {
                break;
            }

            _producer.SendMessage(message!);

            Console.WriteLine("SEND!");
        }
        
        return Task.CompletedTask;
    }
}