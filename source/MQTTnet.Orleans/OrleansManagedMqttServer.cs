using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using Newtonsoft.Json;
using Orleans;
using Orleans.Streams;

namespace MQTTnet.Orleans
{
    public class OrleansManagedMqttServer : MqttServer, IHostedService
    {
        readonly IClusterClient _clusterClient;
        readonly IMqttServerOptions _mqttServerOptions;
        readonly Guid _serverId;
        readonly ILogger<OrleansManagedMqttServer> _appLogger;

        IStreamProvider _streamProvider;
        IAsyncStream<ClientMessage> _serverStream;
        IAsyncStream<AllMessage> _allStream;

        public OrleansManagedMqttServer(IClusterClient clusterClient, IMqttServerOptions mqttServerOptions, IEnumerable<IMqttServerAdapter> adapters, IMqttNetChildLogger logger, ILogger<OrleansManagedMqttServer> appLogger) : base(adapters, logger)
        {
            _serverId = Guid.NewGuid();
            _clusterClient = clusterClient;
            _mqttServerOptions = mqttServerOptions;
            _appLogger = appLogger;

            this.ClientConnected += This_OnClientConnected;
            this.ClientDisconnected += This_OnClientDisconnected;

            appLogger.LogInformation("Orleans Managed MQTT Server has beein instantiated.");
        }
        ~OrleansManagedMqttServer()
        {
            this.ClientConnected -= This_OnClientConnected;
            this.ClientDisconnected -= This_OnClientDisconnected;
        }

        async void This_OnClientConnected(object sender, MqttClientConnectedEventArgs args)
        {
            _appLogger.LogInformation("OnClientConnected fired.");

            if (_streamProvider == null)
            {
                _appLogger.LogInformation("Stream Provider is available... setting up streams.");
                await SetupStreams(args);
            }

            var device = _clusterClient.GetGrain<IDeviceGrain>(Utils.BuildDeviceId(args.ClientId));
            await device.OnConnect(_serverId, args.ClientId);
        }
        async void This_OnClientDisconnected(object sender, MqttClientDisconnectedEventArgs args)
        {
            _appLogger.LogInformation($"Disconnecting {args.ClientId}..");
            var device = _clusterClient.GetGrain<IDeviceGrain>(Utils.BuildDeviceId(args.ClientId));
            await device.OnDisconnect();
        }

        async Task SetupStreams(MqttClientConnectedEventArgs args)
        {
            _streamProvider = _clusterClient.GetStreamProvider(OrleansMqttConstants.StreamProvider);
            _serverStream = _streamProvider.GetStream<ClientMessage>(_serverId, OrleansMqttConstants.ServersStream);
            _allStream = _streamProvider.GetStream<AllMessage>(OrleansMqttConstants.AllStreamId, "ALL");

            var subscribeTasks = new List<Task>()
            {
                _allStream.SubscribeAsync((msg, token) => ProcessAllMessage(msg)),
                _serverStream.SubscribeAsync((msg, token) => ProcessServerMessage(msg))
            };

            await Task.WhenAll(subscribeTasks);
        }

        private async Task ProcessAllMessage(AllMessage msg)
        {
            var clientSessions = await GetClientSessionsStatusAsync();
            var connectedSessions = clientSessions.Where(c => c.IsConnected).ToArray();

            var allTasks = new List<Task>(connectedSessions.Length);

            foreach (var session in connectedSessions)
            {
                if (msg.ExcludeIds == null || !msg.ExcludeIds.Contains(session.ClientId))
                {
                    allTasks.Add(PublishAsync((MqttApplicationMessage)msg.Payload));
                }
            }

            await Task.WhenAll(allTasks);
        }

        private Task ProcessServerMessage(ClientMessage msg)
        {
            _appLogger.LogInformation("Received server message.  Processing... {0}", JsonConvert.SerializeObject(msg.Payload));
            var payload = (MqttApplicationMessage)msg.Payload;
            if (payload != null)
            {
                return this.PublishAsync(payload);
            }
            else
            {
                _appLogger.LogWarning("Could not publish payload.  May not have serialized properly.");
                return Task.CompletedTask;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => base.StartAsync(_mqttServerOptions);

        public Task StopAsync(CancellationToken cancellationToken) => base.StopAsync();
    }
}