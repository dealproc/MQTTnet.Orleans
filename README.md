# Temporarily Archiving This Repository

Due to the fact that the MQTT Server classes are not yet 100% stable, this repository will be archived for the time being.  If/when the libraries are finally stabilized, this repository will be re-activated and updated to use the newer MQTTnet libraries (possibly the v3.x libs, but definitely TBD)

If the server libs become stable, and the repository is not yet re-activated, please contact me: duncan16 at comcast.net and I will put the time in to update this library to support the server architecture for those that are using Orleans as a backplane.

Thanks!

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

## Nginx Ingress Configuration ##

If using WebSockets, ensure that you not only set the kubernetes ingress to enable websockets with the `nginx.ingress.kubernetes.io/websocket-services` annotation, but also include the affinity, session cookie hash, and session cookie neame annotations.  Without these items, your clients will not be kicked between reloads of the mqtt websocket service.

```yaml
apiVersion: extensions/v1beta1
kind: Ingress
metadata:
  name: {your service}
  annotations:
    kubernetes.io/ingress.class: nginx
    kubernetes.io/tls-acme: "true"
    nginx.org/websocket-services: "{your service}"
    nginx.ingress.kubernetes.io/websocket-services: "{your service}"
    nginx.ingress.kubernetes.io/affinity: cookie
    nginx.ingress.kubernetes.io/session-cookie-hash: sha1
    nginx.ingress.kubernetes.io/session-cookie-name: REALTIMESERVERID
    nginx.ingress.kubernetes.io/ssl-ciphers: "ECDH+AESGCM:ECDH+AES256:ECDH+AES128:DH+3DES:!ADH:!AECDH:!MD5"
    nginx.ingress.kubernetes.io/proxy-buffer-size: "64k"
    nginx.ingress.kubernetes.io/proxy-buffering: "on"
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  tls:
  - hosts: 
    - host1.domain.com
    - host2.domain.com
    - host3.domain.com
    secretName: tls-secret
  rules:
  - host: host1.domain.com
    http:
      paths:
      - path: /bus
        backend:
          serviceName: {your service}
          servicePort: 5000
```