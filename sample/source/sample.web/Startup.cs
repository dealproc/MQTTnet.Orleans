using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet.AspNetCore;

using Orleans;
using Orleans.Configuration;

using sample.grains;

namespace sample.web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IClusterClient>(CreateClusterClient);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
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
