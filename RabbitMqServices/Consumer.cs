using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqServices;

public class Consumer : IHostedService
{
    private readonly ILogger<Consumer> _logger;
    private readonly IConnection? _connection;
    private readonly string? _queue;
    private readonly IModel _channel;
    private Timer? _timer;

    public Consumer(IConfiguration configuration, ILogger<Consumer> logger)
    {
        _logger = logger;
        _queue = configuration["Queue"];
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri($"{configuration["StringConnection"]}"),
            ClientProvidedName = configuration["ClientProvidedName"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(configuration["ExchangeName"], ExchangeType.Direct);
        _channel.QueueDeclare(_queue, false, false, false, null);
        _channel.QueueBind(_queue, configuration["ExchangeName"], 
            configuration["RoutingKey"], null);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(
            _ => SendMail(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(5));
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Receiver stopped");

        _timer!.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
        
        _connection!.Close();
        _channel.Close();
        
        _connection.Dispose();
        _channel.Dispose();
        
        return Task.CompletedTask;
    }

    private void SendMail()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
        
            _logger.LogInformation("New message received");
            _logger.LogInformation("Message: {Message}", message);
        
            _channel.BasicAck(ea.DeliveryTag, false);
        };
        
        _channel.BasicConsume(_queue,false, consumer);
    }
}