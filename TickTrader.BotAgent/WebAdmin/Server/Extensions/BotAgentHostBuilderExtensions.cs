using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TickTrader.Algo.Server;
using TickTrader.Algo.ServerControl;
using TickTrader.Algo.ServerControl.Grpc;
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
                services.AddSingleton<IAlgoServerLocal>(_ => botAgent.Server);
                services.AddHostedService<BotAgentHostedService>();
            });
            return builder;
        }

        public static IHostBuilder AddProtocolServer(this IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ServerSettings>(s => s.GetRequiredService<IConfiguration>().GetProtocolServerSettings());
                services.AddSingleton<IJwtProvider, JwtProviderAdapter>();
                services.AddSingleton<IAlgoServerProvider, BotAgentServerAdapter>();
                services.AddSingleton<IProtocolServer, GrpcServer>();
                services.AddHostedService<ProtocolHostedService>();
            });
            return builder;
        }
    }
}
