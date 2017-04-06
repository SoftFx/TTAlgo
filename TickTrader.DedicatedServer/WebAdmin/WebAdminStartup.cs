using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using System.IO;
using Microsoft.Extensions.FileProviders;
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

namespace TickTrader.DedicatedServer.WebAdmin
{
    public class WebAdminStartup
    {
        private static readonly string jwtKey = "asdklfhkashdfkhasdkjfhkasdhfkasdhfjkashd";

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
            services.Configure<IConfiguration>(Configuration);
            services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new ViewLocationExpander()));
            services.AddTransient<ITokenOptions>(x => new TokenOptions
            {
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            CoreLoggerFactory.Init(cn => new LoggerAdapter(loggerFactory.CreateLogger(cn)));

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
            app.ObserveDedicatedServer();

            app.UseSwagger();
            app.UseSwaggerUi();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });



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
