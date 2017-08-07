using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using TickTrader.DedicatedServer.WebAdmin.Server.Core;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using TickTrader.Algo.Core;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.WebAdmin.Server.Core.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using TickTrader.DedicatedServer.WebAdmin.Server.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore.Http;

namespace TickTrader.DedicatedServer.WebAdmin
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("WebAdmin/appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; private set; }
        private string JwtKey => $"{Configuration.GetSecretKey()}{Configuration.GetCredentials().Password}";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ServerCredentials>(Configuration.GetSection(nameof(AppSettings.Credentials)));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<IConfiguration>(Configuration);
            services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new ViewLocationExpander()));
            services.AddTransient<ITokenOptions>(x => new TokenOptions
            {
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtKey)),
                    SecurityAlgorithms.HmacSha256)
            });
            services.AddTransient<IAuthManager, AuthManager>();

            services.AddSignalR(options => options.Hubs.EnableDetailedErrors = true);
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddSwaggerGen();
            services.AddDedicatedServer()
                    .AddStorageOptions(Configuration.GetSection("PackageStorage"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifeTime, IServiceProvider services)
        {
            appLifeTime.ApplicationStopping.Register(() => Shutdown(services));

            loggerFactory.AddNLog();
            app.AddNLogWeb();

            CoreLoggerFactory.Init(cn => new LoggerAdapter(loggerFactory.CreateLogger(cn)));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "./WebAdmin/webpack.config"
                });
                app.UseSwagger();
                app.UseSwaggerUi();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseJwtAuthentication();

            app.ObserveDedicatedServer();
            app.UseWardenOverBots();

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
            });

            app.UseSignalR();

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

        private void Shutdown(IServiceProvider services)
        {
            var server = services.GetRequiredService<IDedicatedServer>();

            server.ShutdownAsync().Wait(TimeSpan.FromMinutes(1));
        }
    }
}
