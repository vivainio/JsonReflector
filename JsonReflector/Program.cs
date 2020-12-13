using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonReflector;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReflectorServer
{
    public class AppIntegration : IDispatcherIntegration
    {
        public Session CreateSession()
        {
            // no services to add just now
            return new Session();
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(sc =>
                {
                    sc.AddSingleton<IDispatcherIntegration, AppIntegration>();
                    var dispatcher = new Dispatcher();
                    dispatcher.RegisterTypes(new[] { typeof(DemoDispatchClass) });
                    sc.AddSingleton(dispatcher);
                });
            
    }
}
