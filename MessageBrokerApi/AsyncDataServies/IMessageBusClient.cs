using MessageBrokerApi.Models;

namespace MessageBrokerApi.AsyncDataServies
{
    public interface IMessageBusClient
    {
        void PublishNewMessageBroker(RabbitMQMessage rabbitMQMessage, string ReciverUserID);
        void ConsumeRabbitMQMessage (string UserID);
    }
}