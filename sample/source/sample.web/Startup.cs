using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using sample.grains;
using MQTTnet.AspNetCore;

namespace sample.web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClusterClient>(CreateClusterClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }

        static IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            var log = serviceProvider.GetRequiredService<ILogger<IClusterClient>>();

            var client = default(IClusterClient);

            client = new ClientBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "sample";
                    options.ServiceId = "service";
                })
                .UseMqtt()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IMyLogicalDevice).Assembly).WithReferences())
                .AddClusterConnectionLostHandler((s, evt) =>
                {
                    // TODO: Add restart logic for MQTT services, if possible.  Otherwise, have this service shut itself down and let k8s restart it.

                    // try
                    // {
                    //     await client?.Connect(RetryFilter);
                    // }
                    // catch (Exception exc)
                    // {
                    //     log.LogWarning("Attempt to reconnect to orleans cluster failed: {0}", exc);
                    // }
                })
                .ConfigureServices((services) =>
                {
                    services.AddLogging();
                })
                .Build();


            client.Connect(RetryFilter).GetAwaiter().GetResult();
            return client;

            async Task<bool> RetryFilter(Exception exc)
            {
                log.LogWarning("Exception while attempting to connect to Orleans cluster: {0}", exc);
                await Task.Delay(TimeSpan.FromSeconds(2));
                return true;
            }
        }
    }
}
