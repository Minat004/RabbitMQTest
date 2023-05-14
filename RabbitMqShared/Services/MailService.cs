using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using RabbitMqShared.Models;

namespace RabbitMqShared.Services;

public class MailService : IMailService
{
    private readonly IConfiguration _configuration;

    public MailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendMail(MailBody? mailBody)
    {
        var mail = new MimeMessage();
        mail.From.Add(MailboxAddress.Parse(_configuration["MailName"]));
        mail.To.Add(MailboxAddress.Parse(mailBody!.To));
        mail.Subject = mailBody.Subject;
        mail.Body = new TextPart(TextFormat.Html) { Text = mailBody.Body };
        
        using (var smtpClient = new SmtpClient())
        {
            smtpClient.Connect(_configuration["MailHost"], 587, SecureSocketOptions.StartTls);
            smtpClient.Authenticate(_configuration["MailName"], _configuration["MailPass"]);
            smtpClient.Send(mail);
            smtpClient.Disconnect(true);
        }
    }
}