using RabbitMqShared.Models;

namespace RabbitMqShared.Services;

public interface IMailService
{
    public void SendMail(MailBody? mailBody);
}