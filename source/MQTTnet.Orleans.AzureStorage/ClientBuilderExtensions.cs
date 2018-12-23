using System;

using MQTTnet.Orleans;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.Streams.AzureQueue;

namespace MQTTnet.AspNetCore
{
    public static class ClientBuilderAzureQueueStorageExtensions
    {
        // public static IClientBuilder UseMqttWithAzureStorage<TDataAdapter>(this IClientBuilder builder, string name, Action<ClusterClientAzureQueueStreamConfigurator<TDataAdapter>> configure)
        //     where TDataAdapter : IAzureQueueDataAdapter
        // {
        //     var configurator = new ClusterClientAzureQueueStreamConfigurator<TDataAdapter>(name, builder);
        //     configure?.Invoke(configurator);
        //     return builder;
        // }

        public static IClientBuilder UseMqttWithAzureStorage<TDataAdapter>(this IClientBuilder builder, Action<OptionsBuilder<AzureQueueOptions>> configureOptions)
            where TDataAdapter : IAzureQueueDataAdapter
        {
            return builder.AddAzureQueueStreams<TDataAdapter>(OrleansMqttConstants.StreamProvider, configureOptions)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IDeviceGrain).Assembly).WithReferences());
        }
    }
}