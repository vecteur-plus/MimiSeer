using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using SupervisorProcessing.Service.Collecte;
using SupervisorProcessing.Utils;
using System;

namespace WebSocketServerWebfollow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;

            
            CollectManager serviceCollect = services.GetService<CollectManager>();

            //start collect with init then loop action every x second
            serviceCollect.StartCollect(services.GetService<IOptions<ConfigTimeCollectLoop>>().Value.TimeCollectLoopWithoutUser);
      
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}