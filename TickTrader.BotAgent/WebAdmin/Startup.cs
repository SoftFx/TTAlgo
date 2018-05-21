using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using TickTrader.BotAgent.WebAdmin.Server.Core;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.Algo.Core;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore.Http;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.WebAdmin.Server.Protocol;

namespace TickTrader.BotAgent.WebAdmin
{
    public class Startup
    {
        private ProtocolServer _protocolServer;

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
            services.AddStorageOptions(Configuration.GetSection("PackageStorage"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifeTime, IServiceProvider services)
        {
            appLifeTime.ApplicationStopping.Register(() => Shutdown(services));

            loggerFactory.AddNLog();
            app.AddNLogWeb();

            LogUnhandledExceptions(loggerFactory);

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

            app.ObserveBotAgent();
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

            //_protocolServer = new ProtocolServer(new BotAgentServer(services, Configuration), Configuration.GetProtocolServerSettings(env.ContentRootPath));
            //_protocolServer.Start();
        }

        private void Shutdown(IServiceProvider services)
        {
            var server = services.GetRequiredService<IBotAgent>();

            //_protocolServer.Stop();
            server.ShutdownAsync().Wait(TimeSpan.FromMinutes(1));
        }

        private void LogUnhandledExceptions(ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("AppDomain.UnhandledException");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                logger.LogCritical($"(This is definitely a bug!) {e.ExceptionObject}");
            };
        }
    }
}
