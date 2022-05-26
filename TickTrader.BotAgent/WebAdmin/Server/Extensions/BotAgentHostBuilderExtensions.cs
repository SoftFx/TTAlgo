using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.PublicAPI.Adapter;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.HostedServices;
using TickTrader.BotAgent.WebAdmin.Server.Protocol;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BotAgentHostBuilderExtensions
    {
        public static IHostBuilder AddBotAgent(this IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var botAgent = ServerModel.Load();
                services.AddSingleton<IBotAgent>(_ => botAgent);
                services.AddSingleton(_ => botAgent.Server);
                services.AddSingleton<IAlgoServerApi>(_ => botAgent.Server);
                services.AddHostedService<AlgoServerHostedService>();
            });
            return builder;
        }

        public static IHostBuilder AddProtocolServer(this IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ServerSettings>(s => s.GetRequiredService<IConfiguration>().GetProtocolServerSettings());
                services.AddSingleton<IJwtProvider, JwtProviderAdapter>();
                services.AddSingleton<PublicApiServer>();
                services.AddHostedService<PublicApiHostedService>();
            });
            return builder;
        }
    }
}
