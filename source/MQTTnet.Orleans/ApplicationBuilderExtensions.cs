using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using MQTTnet.Server;

namespace MQTTnet.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// For use within your Mqtt Server host, this will wire-up the Orleans-specific implementation of the Mqtt Server.
        /// </summary>
        public static IApplicationBuilder UseOrleansMqttServer(this IApplicationBuilder app, Action<IMqttServer> configure)
        {
            var server = app.ApplicationServices.GetRequiredService<IMqttServer>();

            configure(server);

            return app;
        }
    }
}