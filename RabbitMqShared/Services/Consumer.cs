using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqShared.Models;

namespace RabbitMqShared.Services;

public class Consumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<Consumer> _logger;
    private readonly IMailService _mail;
    private IConnection? _connection;
    private readonly string? _queue;
    private IModel? _channel;

    public Consumer(IConfiguration configuration, ILogger<Consumer> logger, IMailService mail)
    {
        _configuration = configuration;
        _logger = logger;
        _mail = mail;
        _queue = configuration["Queue"];

        try
        {
            Initialize();
        }
        catch (Exception e)
        {
            _logger.LogWarning("Connection failed {Error}", e.Message);
        }
    }

    private void Initialize()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri($"{_configuration["StringConnection"]}"),
            ClientProvidedName = _configuration["ClientProvidedName"]
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(_configuration["ExchangeName"], ExchangeType.Direct);
        _channel.QueueDeclare(_queue, false, false, false, null);
        _channel.QueueBind(_queue, _configuration["ExchangeName"],
            _configuration["RoutingKey"], null);
        
        _logger.LogInformation("Connected at {Time}", DateTimeOffset.Now);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receiver running at: {Time}", DateTimeOffset.Now);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            SendMail();
            
            await Task.Delay(2000, cancellationToken);
        }
    }

    private void SendMail()
    {
        if (_channel is null)
        {
            _logger.LogInformation("Connection failed at: {Time}", DateTimeOffset.Now);
            _logger.LogInformation("Try connection again");

            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                _logger.LogWarning("Connection failed {Error}", e.Message);
            }
            
            return;
        }
        
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
        
            _logger.LogInformation("New message received");
            _logger.LogInformation("Message: {Message}", message);

            var mailBody = JsonSerializer.Deserialize<MailBody>(message);
            
            _mail.SendMail(mailBody);
        
            _channel!.BasicAck(ea.DeliveryTag, false);
        };
        
        _channel.BasicConsume(_queue,false, consumer);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receiver stopped at: {Time}", DateTimeOffset.Now);
        
        _connection!.Close();
        _channel!.Close();

        await Task.CompletedTask;
    }
}