using MQTTnet.Orleans;

using Orleans;
using Orleans.Hosting;

namespace MQTTnet.AspNetCore
{
    public static class SiloHostBuilderExtensions
    {
        /// <summary>
        /// For use with Orleans Silos, this wires up the required storage and stream connections to allow Device grains to orchestrate communications
        /// with Orleans Clients which host MQTTnet server instances.
        /// </summary>
        public static ISiloHostBuilder UseMqtt(this ISiloHostBuilder builder, bool useFireAndForgetDelivery = false)
        {
            try { builder = builder.AddMemoryGrainStorage("PubSubStore"); }
            catch { /** PubSubStore was already added. Do nothing. **/ }

            builder = builder.AddMemoryGrainStorage(OrleansMqttConstants.StorageProvider)
                .AddSimpleMessageStreamProvider(OrleansMqttConstants.StreamProvider, opt => opt.FireAndForgetDelivery = useFireAndForgetDelivery)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(DeviceGrain).Assembly).WithReferences());

            return builder;
        }

        /// <summary>
        /// For use with Orleans Clients, this wires-up the required items for clients to talk with Device grains.
        /// </summary>
        public static IClientBuilder UseMqtt(this IClientBuilder builder, bool useFireAndForgetDelivery = false)
        {
            return builder.AddSimpleMessageStreamProvider(OrleansMqttConstants.StreamProvider, opt => opt.FireAndForgetDelivery = useFireAndForgetDelivery)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IDeviceGrain).Assembly).WithReferences());
        }
    }
}