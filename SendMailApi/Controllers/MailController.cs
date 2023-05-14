using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Mvc;
using RabbitMqShared.Models;
using RabbitMqShared.Services;

namespace SendMailApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly IProducer _producer;
    private readonly ILogger _logger;

    public MailController(IProducer producer, ILogger logger)
    {
        _producer = producer;
        _logger = logger;
    }

    [Route("[action]")]
    [HttpPost]
    public IActionResult SendMessage(MailBody mailBody)
    {
        try
        {
            _producer.SendMessage(mailBody);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Producer failed: {Error}", e.Message);
        }
        
        return Ok();
    }
}