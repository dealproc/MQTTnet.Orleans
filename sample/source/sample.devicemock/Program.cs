using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace sample.devicemock
{
    class Program
    {
        static TopicFilter[] FILTERS = new TopicFilter[]{
            new TopicFilterBuilder().WithTopic("device1/ping").WithExactlyOnceQoS().Build()
        };
        static IManagedMqttClient Client;


        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(
                    new MqttClientOptionsBuilder()
                        .WithClientId("device1")
                        .WithCleanSession()
                        .WithKeepAlivePeriod(TimeSpan.FromHours(24))
                        .WithKeepAliveSendInterval(TimeSpan.FromSeconds(5))
                        .WithTcpServer("localhost", 1833)
                        .Build()
                )
                .Build();

            Client = new MqttFactory().CreateManagedMqttClient();
            Client.Connected += (s, e) => Console.WriteLine("Connected");
            Client.Disconnected += (s, e) => Console.WriteLine("Disconnected.");
            Client.ApplicationMessageReceived += (s, e) =>
            {
                switch (e.ApplicationMessage.Topic)
                {
                    case "device1/ping":
                        Console.WriteLine("Received Ping Request.");
                        break;
                }
            };

            await Client.SubscribeAsync(FILTERS);
            await Client.StartAsync(options);

            Console.WriteLine("Press enter to end program.");
            Console.ReadLine();
        }
    }
}
