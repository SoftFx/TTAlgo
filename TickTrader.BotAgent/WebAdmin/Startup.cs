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
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using Microsoft.AspNetCore.Http;
using TickTrader.Algo.Protocol;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TickTrader.BotAgent.WebAdmin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }
        private string JwtKey => Configuration.GetJwtKey();

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

            // .NET Core SDK 2.1.4 has broken core-1.1 apps compatibility with net4xx targets
            // This workaround should avoid problematic code paths
            // Upgrading to core-2.1 should resolve issue completely
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(typeof(Startup).Assembly));
            services.AddSingleton(manager);

            services.AddSignalR(options => options.Hubs.EnableDetailedErrors = true);
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddSwaggerGen();
            services.AddStorageOptions(Configuration.GetSection("PackageStorage"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifeTime, IServiceProvider services)
        {
            appLifeTime.ApplicationStopping.Register(() => Shutdown(services));

            LogUnhandledExceptions(services);

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
            app.UseHttpsRedirection();

            app.UseJwtAuthentication();

            app.ObserveBotAgent();
            app.UseWardenOverBots();

            app.UseAuthentication();

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
            var server = services.GetRequiredService<IBotAgent>();
            var protocolServer = services.GetRequiredService<ProtocolServer>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            protocolServer.Stop();
            server.ShutdownAsync().Wait(TimeSpan.FromMinutes(1));

            var logger = loggerFactory.CreateLogger(nameof(Startup));
            logger.LogInformation("Web host stopped");
        }

        private void LogUnhandledExceptions(IServiceProvider services)
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("AppDomain.UnhandledException");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                logger.LogError($"(This is definitely a bug!) {e.ExceptionObject}");
            };
        }
    }
}
