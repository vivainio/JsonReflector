// customize this app to add your own types

using System;
using JsonReflector;

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

namespace JsonReflectorDemoApp
{
    public class AppIntegration : IDispatcherIntegration
    {
        public Session CreateSession()
        {
            return new Session();
        }
    }

    // simple demo program. Fork your ows for better customization please
    public static class Program
    {
        public static Type[] TypesToInstall = new[]
        {
            typeof(DemoDispatchClass),
            typeof(DemoOtherClass),
        };


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureServices(sc =>
                {
                    sc.AddSingleton<IDispatcherIntegration, AppIntegration>();
                    var dispatcher = new Dispatcher();
                    dispatcher.RegisterTypes(TypesToInstall, prefix: "App.");
                    sc.AddSingleton(dispatcher);
                });

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}
    
