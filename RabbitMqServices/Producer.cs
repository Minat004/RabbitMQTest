using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace RabbitMqServices;

public class Producer : IProducer
{
    private readonly string? _connection;
    private readonly string? _client;
    private readonly string? _exchange;
    private readonly string? _routingKey;
    private readonly string? _queue;

    public Producer(IConfiguration configuration)
    {
        _connection = configuration["StringConnection"];
        _client = configuration["ClientProvidedName"];
        _exchange = configuration["ExchangeName"];
        _routingKey = configuration["RoutingKey"];
        _queue = configuration["Queue"];
    }

    public void SendMessage(string message)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri($"{_connection}"),
            ClientProvidedName = _client
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(_exchange, ExchangeType.Direct);
            channel.QueueDeclare(_queue, false, false, false, null);
            channel.QueueBind(_queue, _exchange, _routingKey, null);

            var body = Encoding.UTF8.GetBytes(message);
            
            channel.BasicPublish(_exchange, _routingKey, null, body);
        }
    }

    public void SendMessage(object json)
    {
        var message = JsonSerializer.Serialize(json);
        SendMessage(message);
    }
}