# MQTTnet.Orleans

Although MQTTnet has both a client and server component within its library, there is no ability natively to load-balance the work across multiple host nodes.  MQTTnet.Orleans is an implementation using Orleans as a backplane to enable load-balancing scenarios, with the added benefit of allowing any Orleans Client the ability to address the device without needing a significant amount of infrastructure.

## Orleans Silo Configuration ##

Note: Most of the configuration is done for you already. As the implementor, call `.UseMqtt()` on the `SiloHostBuilder` object.

```CSharp
static async Task Main(string[] args)
{
    // other config stuff.
    // ...
    //

    var silo = new SiloHostBuilder()
        .UseMqtt()
        .Build();
    
    // other config stuff.
    // ...
    //
}


```

Note-2: It may be beneficial to create a grain which abstracts or otherwise encapsulates the MQTT calls.  We have done this in our implementation project which is where this project is rooted from.

## Orleans Client Configuration ##

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHostedOrleansMqttServer((builder) =>
        {
            builder
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(9876)
                .WithConnectionValidator(async (ctx) =>
                {
                    // any connection validation code you need goes in here.
                });
        })
        .AddMqttTcpServerAdapter(); // can be tcp server; websocket bits; etc.
}

```