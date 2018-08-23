namespace MQTTnet.Orleans
{
    public static class Utils
    {
        public static string BuildDeviceId(string clientId) => $"dev-{clientId}".ToLower();
    }
}