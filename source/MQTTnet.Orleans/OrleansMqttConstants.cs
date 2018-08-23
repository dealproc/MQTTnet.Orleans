using System;

namespace MQTTnet.Orleans
{
    public static class OrleansMqttConstants
    {
        public const string StreamProvider = "ORLEANS_MQTT_STREAM_PROVIDER";
        public const string ServersStream = "SERVERS_STREAM";
        public static readonly Guid AllStreamId = Guid.Parse("C0BAAF7E-7FB2-4DBF-8478-ED7BE3154E5A");
        public const string StorageProvider = "ORLEANS_MQTT_STORAGE_PROVIDER";
        public static readonly Guid ClientDisconnectStreamId = Guid.Parse("D653CE58-8755-44EF-9DD2-EE0C2010BB3F");

    }
}