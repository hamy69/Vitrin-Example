using System;
using Microsoft.Extensions.Configuration;
using MessageBrokerApi.Models;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client.Events;

namespace MessageBrokerApi.AsyncDataServies
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration, string UserID)
        {
            // Build connection to RabbitMQ
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // declare user queue
                _channel.ExchangeDeclare(exchange: "direct-exchange", type: ExchangeType.Direct, arguments: new Dictionary<string, object> { { "x-message-ttl", 30000 } });

                _connection.ConnectionShutdown += RabbitMQConnectionShutdown;

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message} ");
            }
        }

        public void PublishNewMessageBroker(RabbitMQMessage rabbitMQMessage, string ReciverUserID)
        {
            var message = JsonSerializer.Serialize(rabbitMQMessage);
            if (_connection.IsOpen)
            {
                Console.WriteLine($"--> RabbitMQ Connection open, sending message ...");
                // Send direct the message
                SendMessage(message, $"User_{ReciverUserID}");
            }
            else
            {
                Console.WriteLine($"--> RabbitMQ Connection is closed, not sending ");
            }
        }
        private void SendMessage(string message, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                exchange: "direct-exchange",
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
            Console.WriteLine($"--> {routingKey} have sent :{message}");
        }
        private void RabbitMQConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine($"--> RabbitMQ Connection Shutdown");
        }
        public void Disposed()
        {
            Console.WriteLine($"--> Massage Bus disposed!");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        public void ConsumeRabbitMQMessage(string UserID)
        {
            _channel.QueueDeclare(
                queue: "direct-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );
            _channel.QueueBind(
                queue:"demo-direct-queue",
                exchange:"demo-direct-exchange",
                routingKey:"User_{UserID}"
                );
            _channel.BasicQos(0, 10, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                RabbitMQMessage rabbitMQMessage = JsonSerializer.Deserialize<RabbitMQMessage>(message);
                //TODO: Store resived messages into redit cash for send back via controler
                Console.WriteLine($"--> New message is resived by user {rabbitMQMessage.reciverUserID} from user {rabbitMQMessage.senderUserID} :{rabbitMQMessage.MessageBody}");
            };

            _channel.BasicConsume("direct-queue", true, consumer);
            Console.WriteLine("--> User_{UserID} start Consum!");
        }
    }
}