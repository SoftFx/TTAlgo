using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using TickTrader.DedicatedServer.DS;
using System.IO;
using Microsoft.Extensions.FileProviders;
using TickTrader.DedicatedServer.WebAdmin.Server.Core;
using TickTrader.DedicatedServer.DS.Repository;
using TickTrader.DedicatedServer.DS.Repository.Interface;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace TickTrader.DedicatedServer.WebAdmin
{
    public class WebAdminStartup
    {
        public WebAdminStartup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("WebAdmin/appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"WebAdmin/appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<PackageStorageSettings>(Configuration.GetSection("PackageStorage"));

            services.AddTransient<IPackageStorage, PackageStorage>();
            services.AddSingleton<IDedicatedServer, DedicatedServerProvider>();

            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new SignalRContractResolver();

            services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new ViewLocationExpander()));

            services.AddSignalR(options => options.Hubs.EnableDetailedErrors = true);
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "./WebAdmin/webpack.config"
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"WebAdmin/wwwroot")),
            });

            app.UseWebSockets();
            app.UseSignalR();


            app.UseSwagger();
            app.UseSwaggerUi();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
