namespace RabbitMqServices;

public interface IProducer
{
    public void SendMessage(string message);

    public void SendMessage(object json);
}