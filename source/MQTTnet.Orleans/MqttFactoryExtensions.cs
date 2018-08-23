using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Diagnostics;
using MQTTnet.Implementations;
using MQTTnet.Server;

using Orleans;

namespace MQTTnet.Orleans
{
    public static class MqttFactoryExtensions
    {
        public static IMqttServer CreateOrleansMqttSever(this MqttFactory factory, IClusterClient clusterClient, IMqttServerOptions options, ILoggerFactory appLoggerFactory)
        {
            if (clusterClient == null) { throw new ArgumentNullException(nameof(clusterClient)); }

            var logger = new MqttNetLogger();

            return CreateOrleansMqttSever(factory, clusterClient, options, logger, appLoggerFactory.CreateLogger<OrleansManagedMqttServer>());
        }

        private static IMqttServer CreateOrleansMqttSever(this MqttFactory factory, IClusterClient clusterClient, IMqttServerOptions options, IMqttNetLogger logger, ILogger<OrleansManagedMqttServer> appLogger)
        {
            if (clusterClient == null) { throw new ArgumentNullException(nameof(clusterClient)); }
            if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

            return CreateOrleansMqttSever(factory, clusterClient, options, new List<IMqttServerAdapter> { new MqttTcpServerAdapter(logger.CreateChildLogger()) }, logger, appLogger);
        }

        private static IMqttServer CreateOrleansMqttSever(this MqttFactory factory, IClusterClient clusterClient, IMqttServerOptions options, List<IMqttServerAdapter> adapters, IMqttNetLogger logger, ILogger<OrleansManagedMqttServer> appLogger)
        {
            if (clusterClient == null) { throw new ArgumentNullException(nameof(clusterClient)); }
            if (adapters == null) { throw new ArgumentNullException(nameof(adapters)); }
            if (logger == null) { throw new ArgumentNullException(nameof(logger)); }

            return new OrleansManagedMqttServer(clusterClient, options, adapters, logger.CreateChildLogger(), appLogger);
        }
    }
}