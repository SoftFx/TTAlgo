using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using TickTrader.BotAgent.WebAdmin.Server.Core;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Hubs;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using TickTrader.BotAgent.WebAdmin.Server.Settings;

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

            services.AddSignalR(options => options.EnableDetailedErrors = true)
                .AddJsonProtocol(o =>
                {
                    o.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    o.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });

            services.AddControllersWithViews().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "AlgoServer WebAPI", Version = "v1" }));

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

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //{
                //    HotModuleReplacement = true,
                //    HotModuleReplacementEndpoint = "/dist/__webpack_hmr",
                //    ConfigFile = "./WebAdmin/webpack.config"
                //});
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlgoServer WebAPI v1"));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseJwtAuthentication();

            app.ObserveBotAgent();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BAFeed>("/signalr").RequireAuthorization();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "spa-fallback",
                    pattern: "{*clientRoute}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
