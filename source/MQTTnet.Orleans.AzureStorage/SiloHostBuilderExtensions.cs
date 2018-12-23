using System;

using MQTTnet.Orleans;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.Streams.AzureQueue;

namespace MQTTnet.AspNetCore
{
    public static class SiloHostBuilderAzureQueueStorageExtensions
    {
        // public static ISiloHostBuilder AddAzureQueueStreams<TDataAdapter>(this ISiloHostBuilder builder, string name, Action<SiloAzureQueueStreamConfigurator<TDataAdapter>> config)
        //     where TDataAdapter : IAzureQueueDataAdapter
        // {
        //     var configurator = new SiloAzureQueueStreamConfigurator<TDataAdapter>(name, builder);
        //     config?.Invoke(configurator);
        //     return builder;
        // }
        public static ISiloHostBuilder UseMqttWithAzureStorage<TDataAdapter>(this ISiloHostBuilder builder, Action<OptionsBuilder<AzureQueueOptions>> configureOptions)
            where TDataAdapter : IAzureQueueDataAdapter
        {
            try { builder = builder.AddMemoryGrainStorage("PubSubStore"); }
            catch { }

            builder = builder.AddMemoryGrainStorage(OrleansMqttConstants.StorageProvider)
                .AddAzureQueueStreams<TDataAdapter>(OrleansMqttConstants.StreamProvider, configureOptions)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(DeviceGrain).Assembly).WithReferences());

            return builder;
        }
    }
}
