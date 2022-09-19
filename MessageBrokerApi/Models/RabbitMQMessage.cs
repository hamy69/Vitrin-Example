namespace MessageBrokerApi.Models
{
    public class RabbitMQMessage
    {
        public int senderUserID { get; set; }
        public int reciverUserID { get; set; }
        public string? MessageBody { get; set; }
    }
}