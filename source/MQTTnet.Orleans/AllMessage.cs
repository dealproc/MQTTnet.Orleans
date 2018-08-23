using System.Collections.Generic;

namespace MQTTnet.Orleans
{
    /// <summary>
    /// [May be obsolete] - Allows a message to be sent to all clients, where you subsequently exclude ids as necessary.
    /// </summary>
    public class AllMessage
    {
        public IReadOnlyList<string> ExcludeIds { get; set; }
        public object Payload { get; set; }
    }
}