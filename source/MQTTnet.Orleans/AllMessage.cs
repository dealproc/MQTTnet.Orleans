using System.Collections.Generic;

namespace MQTTnet.Orleans
{
    public class AllMessage
    {
        public IReadOnlyList<string> ExcludeIds { get; set; }
        public object Payload { get; set; }
    }
}