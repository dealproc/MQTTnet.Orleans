using System;
using System.Threading.Tasks;

using MQTTnet;

using Orleans;
using Orleans.Providers;
using Orleans.Streams;

namespace MQTTnet.Orleans
{
    /// <summary>
    /// Represents a device that connects to an Mqtt Server.
    /// </summary>
    public interface IDeviceGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Method that executes upon a device connecting the the Mqtt Server.  Basic setup is to connect it within a grain to allow it to be addressed by the silo.
        /// </summary>
        Task OnConnect(Guid serverId, string connectionId);

        /// <summary>
        /// When the device disconnects, this method is executed to do cleanup of the connection and supporting grain objects.
        /// </summary>
        Task OnDisconnect();

        /// <summary>
        /// Sends a message to a device.
        /// </summary>
        Task SendMessage(MqttApplicationMessage message);
    }

    /// <summary>
    /// Implementation of a device that is connected to an Mqtt Server.
    /// </summary>
    [StorageProvider(ProviderName = OrleansMqttConstants.StorageProvider)]
    internal class DeviceGrain : Grain<DeviceState>, IDeviceGrain
    {
        IStreamProvider _streamProvider;
        IAsyncStream<ClientMessage> _serverStream;
        IAsyncStream<string> _clientDisconnectStream;

        /// <summary>
        /// Wires-up the grain upon access from within the silo.
        /// </summary>
        public override async Task OnActivateAsync()
        {
            await this.ReadStateAsync();

            _streamProvider = GetStreamProvider(OrleansMqttConstants.StreamProvider);

            if (State.ServerId != Guid.Empty)
            {
                _clientDisconnectStream = _streamProvider.GetStream<string>(OrleansMqttConstants.ClientDisconnectStreamId, State.ConnectionId);
                _serverStream = _streamProvider.GetStream<ClientMessage>(State.ServerId, OrleansMqttConstants.ServersStream);
            }
        }

        /// <summary>
        /// Method that executes upon a device connecting the the Mqtt Server.  Basic setup is to connect it within a grain to allow it to be addressed by the silo.
        /// </summary>
        public Task OnConnect(Guid serverId, string connectionId)
        {
            State.ServerId = serverId;
            State.ConnectionId = connectionId;

            _serverStream = _streamProvider.GetStream<ClientMessage>(State.ServerId, OrleansMqttConstants.ServersStream);
            _clientDisconnectStream = _streamProvider.GetStream<string>(OrleansMqttConstants.ClientDisconnectStreamId, State.ConnectionId);

            return WriteStateAsync();
        }

        /// <summary>
        /// When the device disconnects, this method is executed to do cleanup of the connection and supporting grain objects.
        /// </summary>
        public async Task OnDisconnect()
        {
            if (State.ConnectionId != null)
            {
                await _clientDisconnectStream.OnNextAsync(State.ConnectionId);
            }

            State.ConnectionId = string.Empty;
            State.ServerId = Guid.Empty;

            await ClearStateAsync();

            DeactivateOnIdle();
        }

        /// <summary>
        /// Sends a message to a device.
        /// </summary>
        public Task SendMessage(MqttApplicationMessage message)
        {
            if (this.State.ServerId == Guid.Empty) throw new InvalidOperationException("Client not connected.");
            if (string.IsNullOrWhiteSpace(this.State.ConnectionId)) throw new InvalidOperationException("Client ConnectionId not set.");
            return this._serverStream.OnNextAsync(new ClientMessage { ConnectionId = State.ConnectionId, Payload = message });
        }
    }

    internal class DeviceState
    {
        /// <summary>
        /// The server (id) that this device is connected to.
        /// </summary>
        public Guid ServerId { get; set; }

        /// <summary>
        /// [Should probably be renamed to ClientId] The connection id that is associated with this device.
        /// </summary>
        public string ConnectionId { get; set; }
    }
}