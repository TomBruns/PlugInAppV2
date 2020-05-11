using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using FIS.USESA.POC.Plugins.Shared.Entities;

namespace FIS.USESA.POC.Plugins.Service
{
    /// <summary>
    /// This is the entry point for this assembly
    /// </summary>
    public class Program
    {
        private static HangfireServiceConfigBE _hangfireConfig;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

            // Load config sections
            _hangfireConfig = config.GetSection("hangfireConfig").Get<HangfireServiceConfigBE>();

            CreateHostBuilder(args).Build().Run();
        }

        // set this up to both be:
        //  - installed as a windows service 
        //  - host ASP.NET so we expose the Hangfire Dashboard at http://localhost:5000/hangfire/
        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(BuildURLs(_hangfireConfig));
                })
                //.UseWindowsService()
                ;

        private static string[] BuildURLs(HangfireServiceConfigBE hangfireConfig)
        {
            var urls = new List<string>();

            string protocol = hangfireConfig.IsUseSSL ? @"https" : "http";
            urls.Add($"{protocol}://localhost:{hangfireConfig.DashboardPortNumber}");

            if (hangfireConfig.IsDashboardRemoteAccessEnabled)
            {
                urls.Add($"{protocol}://{Environment.MachineName}:{hangfireConfig.DashboardPortNumber}");
            }

            return urls.ToArray();
        }
    }
}
