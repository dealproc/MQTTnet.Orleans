namespace MQTTnet.Orleans
{
    public class ClientMessage
    {
        /// <summary>
        /// [Should probably be renamed to ClientId] The Connection Id that this message should be sent to.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// What to send the device (its payload of data).
        /// </summary>
        public object Payload { get; set; }
    }
}