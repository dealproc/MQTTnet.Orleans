using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Orleans;

using Orleans;

namespace sample.grains
{
    public interface IMyLogicalDevice : IGrainWithStringKey
    {
        Task PingAsync();
    }

    public class MyLogicalDevice : Grain, IMyLogicalDevice
    {
        IDeviceGrain _device;

        public override Task OnActivateAsync()
        {
            _device = GrainFactory.GetGrain<IDeviceGrain>(this.GetPrimaryKeyString());
            return Task.CompletedTask;
        }

        public async Task PingAsync()
        {
            await _device.SendMessage(new MqttApplicationMessageBuilder()
                .WithExactlyOnceQoS()
                .WithTopic($"{this.GetPrimaryKey()}/Ping")
                .Build()
            );
        }
    }
}