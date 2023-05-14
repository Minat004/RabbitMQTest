using Microsoft.AspNetCore.Mvc;
using RabbitMqShared.Models;
using RabbitMqShared.Services;

namespace SendMailApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly IProducer _producer;

    public MailController(IProducer producer)
    {
        _producer = producer;
    }

    [Route("[action]")]
    [HttpPost]
    public IActionResult SendMessage(MailBody mailBody)
    {
        _producer.SendMessage(mailBody);
        
        return Ok();
    }
}