using System;
using System.Threading.Tasks;

using MQTTnet;

using Orleans;
using Orleans.Providers;
using Orleans.Streams;

namespace MQTTnet.Orleans
{
    public interface IDeviceGrain : IGrainWithStringKey
    {
        Task OnConnect(Guid serverId, string connectionId);
        Task OnDisconnect();
        Task SendMessage(MqttApplicationMessage message);
    }

    [StorageProvider(ProviderName = OrleansMqttConstants.StorageProvider)]
    internal class DeviceGrain : Grain<DeviceState>, IDeviceGrain
    {
        IStreamProvider _streamProvider;
        IAsyncStream<ClientMessage> _serverStream;
        IAsyncStream<string> _clientDisconnectStream;

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

        public Task OnConnect(Guid serverId, string connectionId)
        {
            State.ServerId = serverId;
            State.ConnectionId = connectionId;

            _serverStream = _streamProvider.GetStream<ClientMessage>(State.ServerId, OrleansMqttConstants.ServersStream);
            _clientDisconnectStream = _streamProvider.GetStream<string>(OrleansMqttConstants.ClientDisconnectStreamId, State.ConnectionId);

            return WriteStateAsync();
        }

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

        public Task SendMessage(MqttApplicationMessage message)
        {
            if (this.State.ServerId == Guid.Empty) throw new InvalidOperationException("Client not connected.");
            if (string.IsNullOrWhiteSpace(this.State.ConnectionId)) throw new InvalidOperationException("Client ConnectionId not set.");
            return this._serverStream.OnNextAsync(new ClientMessage { ConnectionId = State.ConnectionId, Payload = message });
        }
    }

    internal class DeviceState
    {
        public Guid ServerId { get; set; }
        public string ConnectionId { get; set; }
    }
}