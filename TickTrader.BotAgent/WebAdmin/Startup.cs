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
using System;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using Microsoft.AspNetCore.Http;
using TickTrader.Algo.Protocol;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using TickTrader.BotAgent.WebAdmin.Server.Hubs;

namespace TickTrader.BotAgent.WebAdmin
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ServerCredentials>(Configuration.GetSection(nameof(AppSettings.Credentials)));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.Configure<IConfiguration>(Configuration);
            services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new ViewLocationExpander()));

            var tokenProvider = new JwtSecurityTokenProvider(Configuration);
            services.AddSingleton<ISecurityTokenProvider, JwtSecurityTokenProvider>(s => tokenProvider);
            services.AddSingleton<IAuthManager, AuthManager>();
            services.AddSingleton<IFdkOptionsProvider, FdkOptionsProvider>();

            services.AddSignalR(options => options.EnableDetailedErrors = true)
                .AddJsonProtocol(o => o.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "AlgoServer WebAPI", Version = "v1" }));
            services.AddStorageOptions(Configuration.GetSection("PackageStorage"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.SecurityTokenValidators.Clear();
                jwtOptions.SecurityTokenValidators.Add(tokenProvider);
                jwtOptions.TokenValidationParameters = tokenProvider.WebValidationParams;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifeTime, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    HotModuleReplacementEndpoint = "/dist/__webpack_hmr",
                    ConfigFile = "./WebAdmin/webpack.config"
                });
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlgoServer WebAPI v1"));
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

            app.UseSignalR(route => route.MapHub<BAFeed>("/signalr"));

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
