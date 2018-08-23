using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MQTTnet.Diagnostics;
using MQTTnet.Server;
using MQTTnet.Orleans;

namespace MQTTnet.AspNetCore
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddHostedOrleansMqttServer(this IServiceCollection services, Action<MqttServerOptionsBuilder> configure)
        {
            var builder = new MqttServerOptionsBuilder();

            configure(builder);

            services.AddSingleton<IMqttServerOptions>(builder.Build());

            services.AddHostedOrleansMqttServer();

            return services;
        }

        private static IServiceCollection AddHostedOrleansMqttServer(this IServiceCollection services)
        {
            var logger = new MqttNetLogger();
            var childLogger = logger.CreateChildLogger();

            services.AddSingleton<IMqttNetLogger>(logger);
            services.AddSingleton(childLogger);
            services.AddSingleton<OrleansManagedMqttServer>();
            services.AddSingleton<IHostedService>(s => s.GetService<OrleansManagedMqttServer>());
            services.AddSingleton<IMqttServer>(s => s.GetService<OrleansManagedMqttServer>());

            return services;
        }
    }
}