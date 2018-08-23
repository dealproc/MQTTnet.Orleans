namespace MQTTnet.Orleans
{
    public class ClientMessage
    {
        public string ConnectionId { get; set; }
        public object Payload { get; set; }
    }
}