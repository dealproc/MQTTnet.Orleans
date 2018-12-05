using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet.AspNetCore;

namespace sample.deviceendpoint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(opts =>
                {
                    opts.ListenAnyIP(1883, l => l.UseMqtt()); // mqtt pipeline
                    opts.ListenAnyIP(5000); // default http pipeline
                })
                .UseStartup<Startup>();
    }
}
