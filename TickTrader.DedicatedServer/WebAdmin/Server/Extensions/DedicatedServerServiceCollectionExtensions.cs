using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using TickTrader.DedicatedServer.DS.Repository;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class DedicatedServerServiceCollectionExtensions
    {
        public static IServiceCollection AddDedicatedServer(this IServiceCollection services)
        {
            services.AddSingleton<IDedicatedServer>(sp => DS.Models.ServerModel.Load(sp.GetService<ILoggerFactory>()));

            return services;
        }

        public static void AddStorageOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<PackageStorageSettings>(configuration);
        }
    }
}
