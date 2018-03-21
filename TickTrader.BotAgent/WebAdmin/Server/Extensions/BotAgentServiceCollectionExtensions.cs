using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Repository;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentServiceCollectionExtensions
    {
        public static IWebHostBuilder AddBotAgent(this IWebHostBuilder builder, IBotAgent agent)
        {
            builder.ConfigureServices(s => s.AddSingleton(agent));
            return builder;
        }

        public static void AddStorageOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<PackageStorageSettings>(configuration);
        }
    }
}
