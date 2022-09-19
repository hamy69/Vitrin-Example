using MessageBrokerApi.AsyncDataServies;
using MessageBrokerApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MessageBrokerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RabbirMQMesengerController : ControllerBase
{
    private readonly ILogger<RabbirMQMesengerController> _logger;
    private readonly IMessageBusClient _messageBusClient;

    public RabbirMQMesengerController(ILogger<RabbirMQMesengerController> logger, IMessageBusClient messageBusClient)
    {
        _logger = logger;
        _messageBusClient = messageBusClient;
    }

    [HttpGet(Name = "GetUserMessage")]
    public void Get(int UserID)
    {
        _messageBusClient.ConsumeRabbitMQMessage(UserID.ToString());
    }
    [HttpGet(Name = "SendMessage")]
    public void Create(RabbitMQMessage rabbitMQMessage)
    {
        //Send Async Message
        try
        {
            _messageBusClient.PublishNewMessageBroker(rabbitMQMessage, rabbitMQMessage.reciverUserID.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
        }
    }
}
