using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Repository;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentServiceCollectionExtensions
    {
        public static IServiceCollection AddBotAgent(this IServiceCollection services)
        {
            services.AddSingleton<IBotAgent>(sp => BA.Models.ServerModel.Load(sp.GetService<ILoggerFactory>()));

            return services;
        }

        public static void AddStorageOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<PackageStorageSettings>(configuration);
        }
    }
}
