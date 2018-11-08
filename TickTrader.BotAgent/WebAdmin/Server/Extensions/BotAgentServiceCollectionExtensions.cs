using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TickTrader.BotAgent.BA.Repository;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentServiceCollectionExtensions
    {
        public static void AddStorageOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<PackageStorageSettings>(configuration);
        }
    }
}
