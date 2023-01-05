using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.DataContext.External;
using SupervisorProcessing.Repository;
using SupervisorProcessing.Service;
using SupervisorProcessing.Service.Collecte;
using SupervisorProcessing.Utils;
using System;
using System.Configuration;
using WebSocketServerWebfollow.Handler;
using WebSocketServerWebfollow.Service;
using WebSocketServerWebfollow.SocketManager;

namespace WebSocketServerWebfollow
{
    public class Startup
    {
        private readonly IConfiguration _Configuration;

        public Startup(IConfiguration configuration_, IWebHostEnvironment env_)
        {

            //get appsetting info in function of EnvironmentName
            //EnvironmentName definded in environment variable
            var configurationBuilder = new ConfigurationBuilder()
              .SetBasePath(env_.ContentRootPath)
              .AddJsonFile($"appsettings.{env_.EnvironmentName}.json", optional: false, reloadOnChange: true);

            _Configuration = configurationBuilder.Build();

            Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(_Configuration)
        .CreateLogger();

            Log.Logger.Information("logger initialisé");
            Log.Logger.Information("configuation de l'environnement de {env} chargé", env_.EnvironmentName);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
           /* services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(Log.Logger, true);
            });*/

            services.AddControllers();

            services.AddOptions<ConfigTimeCollectLoop>().Bind(_Configuration.GetSection("ConfigTimeCollectLoop"));

            services.AddDbContextFactory<DbContextContentGrabber>(options => options.UseSqlite(_Configuration.GetConnectionString("ContentGrabberDatabase")));
            services.AddDbContextFactory<DbContextSiteWeb>(options => options.UseMySQL(_Configuration.GetConnectionString("SiteWebDatabase")));
            services.AddDbContextFactory<DbContextIntern>(options => options.UseInMemoryDatabase(databaseName: "Site"));

            services.AddWebSocketManager();

            services.AddMvc(x => x.EnableEndpointRouting = false);

           
            SupervisorProcessingStartup.AddSingleton(services);
            services.AddSingleton<ServiceFiltre>();
            services.AddSingleton<ProcessingRecepter>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "WebSocketServerWebfollow",
                    //TermsOfService = new Uri("https://example.com/terms"),//To create if required
                    Contact = new OpenApiContact
                    {
                        Name = "Web Grabber Team",
                        Email = "webgrabber.dev@vecteurplus.com"
                    }
                }
                );

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.MapSockets("/resume", serviceProvider.GetService<WebSocketResumeHandler>());
            app.MapSockets("/detail", serviceProvider.GetService<WebSocketDetailedScheduleHandler>());
            app.MapSockets("/InformationRun", serviceProvider.GetService<WebSocketInformationRunHandler>());
            app.MapSockets("/FilterCriteria", serviceProvider.GetService<WebSocketFilterCriteriaHandler>());

            //Important init ProcessingRecepter 
            _ = serviceProvider.GetService<ProcessingRecepter>();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "/swagger/{documentName}/swagger.json";
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSocketServerWebfollow");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}