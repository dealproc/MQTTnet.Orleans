using MQTTnet.Orleans;

using Orleans;
using Orleans.Hosting;

namespace MQTTnet.AspNetCore
{
    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder UseMqtt(this ISiloHostBuilder builder, bool useFireAndForgetDelivery = false)
        {
            try { builder = builder.AddMemoryGrainStorage("PubSubStore"); }
            catch { /** PubSubStore was already added. Do nothing. **/ }

            builder = builder.AddMemoryGrainStorage(OrleansMqttConstants.StorageProvider)
                .AddSimpleMessageStreamProvider(OrleansMqttConstants.StreamProvider, opt => opt.FireAndForgetDelivery = useFireAndForgetDelivery)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(DeviceGrain).Assembly).WithReferences());

            return builder;
        }

        public static IClientBuilder UseMqtt(this IClientBuilder builder, bool useFireAndForgetDelivery = false)
        {
            return builder.AddSimpleMessageStreamProvider(OrleansMqttConstants.StreamProvider, opt => opt.FireAndForgetDelivery = useFireAndForgetDelivery)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IDeviceGrain).Assembly).WithReferences());
        }
    }
}